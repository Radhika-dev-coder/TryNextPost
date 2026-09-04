using Microsoft.Extensions.Logging;
using System.Runtime.Intrinsics.X86;
using TryNextPost.Application.DTO.Ndr;
using TryNextPost.Application.Helpers;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.IServices.Interface.Courier; // Dynamic factory mapping interfaces layer assembly
using TryNextPost.Application.IServices.Interface.INdr;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class.Ndr
{
    public class NdrService : INdrService
    {
        private readonly INdrRepository _ndrRepository;
        private readonly IRtoRepository _rtoRepository;
        private readonly IShipmentRepository _shipmentRepository;
        private readonly ISellerContextService _sellerContextService;
        private readonly ICourierAdapter _courierAdapter;
        private readonly ICourierAdapterFactory _courierAdapterFactory; // Perfectly maintained your dynamic factory contract identifier
        private readonly ILogger<NdrService> _logger;

        public NdrService(
            INdrRepository ndrRepository,
            IRtoRepository rtoRepository,
            IShipmentRepository shipmentRepository,
            ISellerContextService sellerContextService,
            ICourierAdapter courierAdapter,
            ICourierAdapterFactory courierAdapterFactory, // Safely injected at runtime application boundaries
            ILogger<NdrService> logger)
        {
            _ndrRepository = ndrRepository;
            _rtoRepository = rtoRepository;
            _shipmentRepository = shipmentRepository;
            _sellerContextService = sellerContextService;
            _courierAdapter = courierAdapter;
            _courierAdapterFactory = courierAdapterFactory;
            _logger = logger;
        }

        public async Task<NdrListResponse> GetListAsync(string userId, NdrFilterRequest filter)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.ShipmentsView);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);

            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 20 : Math.Min(filter.PageSize, 100);
            var statusFilter = ParseStatusTab(filter.StatusTab);

            var items = await _ndrRepository.GetBySellerFilteredAsync(
                seller.SellerId,
                statusFilter,
                page,
                pageSize,
                filter.SearchQuery,
                filter.FromDate,
                filter.ToDate);
            var totalCount = await _ndrRepository.GetBySellerFilteredCountAsync(
                seller.SellerId,
                statusFilter,
                filter.SearchQuery,
                filter.FromDate,
                filter.ToDate);

            var tabCounts = new NdrTabCounts
            {
                All = await _ndrRepository.GetCountBySellerAndStatusAsync(seller.SellerId, null),
                ActionRequired = await _ndrRepository.GetCountBySellerAndStatusAsync(
                    seller.SellerId, NdrStatus.ActionRequired),
                ActionRequested = await _ndrRepository.GetCountBySellerAndStatusAsync(
                    seller.SellerId, NdrStatus.ActionRequested),
                Delivered = await _ndrRepository.GetCountBySellerAndStatusAsync(
                    seller.SellerId, NdrStatus.Delivered),
                Rto = await _ndrRepository.GetCountBySellerAndStatusAsync(
                    seller.SellerId, NdrStatus.Rto)
            };

            return new NdrListResponse
            {
                Items = items.Select(Map).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TabCounts = tabCounts
            };
        }

        public async Task<NdrListItemResponse> TakeActionAsync(
            string userId,
            long ndrId,
            NdrActionRequest request)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.ShipmentsView);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);

            var ndr = await _ndrRepository.GetByIdAsync(ndrId)
                ?? throw new KeyNotFoundException(SystemMessage.NdrNotFound);

            if (ndr.Shipment?.Order == null || ndr.Shipment.Order.SellerId != seller.SellerId)
                throw new UnauthorizedAccessException(SystemMessage.Unauthorized);

            if (ndr.Status != NdrStatus.ActionRequired && ndr.Status != NdrStatus.ActionRequested)
                throw new InvalidOperationException(SystemMessage.NdrActionNotAllowed);

            var action = string.IsNullOrWhiteSpace(request.Action)
                ? "Reattempt"
                : request.Action.Trim();

            if (IsReattemptAction(action))
            {
                await ApplyReattemptAsync(userId, ndr, request);
            }
            else if (IsRtoAction(action))
            {
                await ApplyRtoAsync(userId, ndr, request);
            }
            else
            {
                throw new InvalidOperationException(SystemMessage.NdrActionInvalid);
            }

            await _ndrRepository.UpdateAsync(ndr);
            await _ndrRepository.SaveChangesAsync();

            return Map(ndr);
        }

        private async Task ApplyReattemptAsync(
            string userId,
            NDR ndr,
            NdrActionRequest request)
        {
            _logger.LogInformation("Processing dynamic multi-courier reattempt routine loop for NdrId: {NdrId}", ndr.NdrId);

            if (ndr.Shipment == null || ndr.Shipment.Courier == null)
            {
                throw new InvalidOperationException("Shipment or active Courier relation stream missing for the selected NDR context identifier.");
            }
            var dynamicAdapter = _courierAdapterFactory.Resolve(ndr.Shipment.Courier.CourierCode);
            bool isCourierAcknowledged = await dynamicAdapter.RequestNdrReAttemptAsync(
                ndr.Shipment.AwbNumber ?? string.Empty,
                string.IsNullOrWhiteSpace(request.Action) ? "Reattempt" : request.Action.Trim(),
                request.Remarks ?? "Seller requested reattempt cycle via dashboard panel.",
                CancellationToken.None);

            if (!isCourierAcknowledged)
            {
                throw new InvalidOperationException($"{ndr.Shipment.Courier.CourierName} physical network gateway rejected the NDR reattempt parameters.");
            }

            ndr.Status = NdrStatus.ActionRequested;
            ndr.UpdatedAt = DateTime.UtcNow;
            ndr.Action = "Reattempt";
            ndr.Remarks = request.Remarks?.Trim() ?? "Action submitted successfully via seller dashboard loop.";
            ndr.UpdatedAt = DateTime.UtcNow;
            ndr.UpdatedBy = userId;

            if (!ShipmentStatusTransitions.IsRtoStatus(ndr.Shipment.Status)
                && ShipmentStatusTransitions.CanTransition(ndr.Shipment.Status, ShipmentStatus.OutForDelivery))
            {
                ndr.Shipment.Status = ShipmentStatus.OutForDelivery;
                ndr.Shipment.UpdatedAt = DateTime.UtcNow;
                ndr.Shipment.UpdatedBy = userId;
                await _shipmentRepository.UpdateAsync(ndr.Shipment);
            }
        }


        private async Task ApplyRtoAsync(
            string userId,
            NDR ndr,
            NdrActionRequest request)
        {
            _logger.LogInformation("Processing dynamic multi-courier RTO dispatch loop for NdrId: {NdrId}", ndr.NdrId);

            if (ndr.Shipment == null || ndr.Shipment.Courier == null)
            {
                throw new InvalidOperationException("Shipment or active Courier relation stream missing for the selected NDR context identifier.");
            }

            var dynamicAdapter = _courierAdapterFactory.Resolve(ndr.Shipment.Courier.CourierCode);
            string targetRtoActionCode = string.Equals(ndr.Shipment.Courier.CourierCode, "DTDC", StringComparison.OrdinalIgnoreCase)
                ? "2"
                : (string.IsNullOrWhiteSpace(request.Action) ? "Rto" : request.Action.Trim());

            bool isCourierAcknowledged = await dynamicAdapter.RequestNdrReAttemptAsync(
                ndr.Shipment.AwbNumber ?? string.Empty,
                targetRtoActionCode,
                request.Remarks ?? "Seller initiated order RTO return transition route.",
                CancellationToken.None);

            if (!isCourierAcknowledged)
            {
                throw new InvalidOperationException($"{ndr.Shipment.Courier.CourierName} integration gateway rejected the RTO request parameters.");
            }
            var reason = !string.IsNullOrWhiteSpace(request.Remarks)
                ? request.Remarks.Trim()
                : (!string.IsNullOrWhiteSpace(ndr.Reason) ? ndr.Reason : "Seller marked RTO");

            ndr.Action = "Rto";
            ndr.Status = NdrStatus.Rto;
            ndr.Remarks = reason;
            ndr.NextAttemptDate = null;
            ndr.UpdatedAt = DateTime.UtcNow;
            ndr.UpdatedBy = userId;

            var existingRto = await _rtoRepository.GetOpenByShipmentIdAsync(ndr.ShipmentId);
            if (existingRto == null)
            {
                await _rtoRepository.AddAsync(new RTO
                {
                    ShipmentId = ndr.ShipmentId,
                    Reason = reason,
                    Status = RtoStatus.Initiated,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                });
            }

            if (ndr.Shipment.Status != ShipmentStatus.RTOInitiated
                && ShipmentStatusTransitions.CanTransition(ndr.Shipment.Status, ShipmentStatus.RTOInitiated))
            {
                ndr.Shipment.Status = ShipmentStatus.RTOInitiated;
                ndr.Shipment.UpdatedAt = DateTime.UtcNow;
                ndr.Shipment.UpdatedBy = userId;
                await _shipmentRepository.UpdateAsync(ndr.Shipment);
            }
            else if (ndr.Shipment.Status != ShipmentStatus.RTOInitiated)
            {
                _logger.LogWarning(
                    "NDR {NdrId} marked RTO but shipment {ShipmentId} status {Status} cannot transition to RTO locally.",
                    ndr.NdrId,
                    ndr.ShipmentId,
                    ndr.Shipment.Status);
            }
        }


        private static bool IsReattemptAction(string action)
            => action.Equals("Reattempt", StringComparison.OrdinalIgnoreCase); 
        private static bool IsRtoAction(string action)
        {
            var key = action.Replace(" ", "", StringComparison.Ordinal).Replace("_", "", StringComparison.Ordinal);
            return key.Equals("Rto", StringComparison.OrdinalIgnoreCase) || key.Equals("MarkRto", StringComparison.OrdinalIgnoreCase); 
        }
        private static NdrStatus? ParseStatusTab(string? statusTab)
        { 
            if (string.IsNullOrWhiteSpace(statusTab) || statusTab.Equals("all", StringComparison.OrdinalIgnoreCase)) 
            return null; var key = statusTab.Trim().ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "");
            return key switch 
            { "actionrequired" or "pending" => NdrStatus.ActionRequired,
              "actionrequested" or "reattemptscheduled" or "reattempt" => NdrStatus.ActionRequested, 
              "delivered" => NdrStatus.Delivered, "rto" or "cancelled" or "canceled" => NdrStatus.Rto, _ =>
              throw new InvalidOperationException(SystemMessage.InvalidNdrStatusTab) };
        }
        private static NdrListItemResponse Map(NDR ndr) { var shipment = ndr.Shipment; var order = shipment?.Order; var product = order?.OrderItems?.FirstOrDefault()?.ProductName ?? string.Empty; var address = BuildAddress(shipment); return new NdrListItemResponse { NdrId = ndr.NdrId, ShipmentId = ndr.ShipmentId, OrderId = order?.OrderId ?? 0, Channel = order?.Channel ?? "Manual", NdrDate = ndr.CreatedAt, OrderRef = order?.OrderRef ?? string.Empty, Product = product, Payment = order?.PaymentMode.ToString() ?? string.Empty, Customer = order?.CustomerName ?? shipment?.DeliveryCustomerName ?? string.Empty, Phone = order?.CustomerMobile ?? shipment?.DeliveryMobile ?? string.Empty, Address = address, Carrier = shipment?.Courier?.CourierName ?? string.Empty, Awb = shipment?.AwbNumber, Status = (int)ndr.Status, StatusName = ToDisplayStatus(ndr.Status), Action = ndr.Action, Reason = ndr.Reason, Remarks = ndr.Remarks, Attempts = ndr.Attempts, NextAttemptDate = ndr.NextAttemptDate }; }
        private static string BuildAddress(TryNextPost.Domain.Entities.Shipment? shipment) { if (shipment == null) return string.Empty; var parts = new[] { shipment.DeliveryAddressLine1, shipment.DeliveryAddressLine2, shipment.DeliveryCity, shipment.DeliveryState, shipment.DeliveryPincode }.Where(p => !string.IsNullOrWhiteSpace(p)); return string.Join(", ", parts); }
        private static string ToDisplayStatus(NdrStatus status) => status switch { NdrStatus.ActionRequired => "Action Required", NdrStatus.ActionRequested => "Action Requested", NdrStatus.Delivered => "Delivered", NdrStatus.Rto => "RTO", _ => status.ToString() };
    }
}