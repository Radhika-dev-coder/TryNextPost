using TryNextPost.Application.DTO.Billing;
using TryNextPost.Application.IServices.Interface.IBilling;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class.Billing
{
    public class CodSettlementService : ICodSettlementService
    {
        private readonly ICODSettlementRepository _codSettlementRepository;
        private readonly IShipmentChargesRepository _shipmentChargesRepository;

        public CodSettlementService(
            ICODSettlementRepository codSettlementRepository,
            IShipmentChargesRepository shipmentChargesRepository)
        {
            _codSettlementRepository = codSettlementRepository;
            _shipmentChargesRepository = shipmentChargesRepository;
        }

        public async Task<CodSettlementAdminListResponse> GetForAdminAsync(CodSettlementAdminFilterRequest filter)
        {
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 50 : Math.Min(filter.PageSize, 200);
            var status = ParseSettlementStatus(filter.Status);

            var (items, total) = await _codSettlementRepository.GetFilteredForAdminAsync(
                filter.SellerId,
                status,
                filter.FromDate,
                filter.ToDate,
                page,
                pageSize);

            var shipmentIds = items.Select(i => i.ShipmentId).Distinct().ToList();
            var freightByShipment = await _shipmentChargesRepository
                .GetSellerChargesByShipmentIdsAsync(shipmentIds);

            var pendingTotal = await _codSettlementRepository.SumByStatusForAdminAsync(
                filter.SellerId, SettlementStatus.Pending);
            var settledTotal = await _codSettlementRepository.SumByStatusForAdminAsync(
                filter.SellerId, SettlementStatus.Settled);

            return new CodSettlementAdminListResponse
            {
                Items = items.Select(c => MapCod(c, freightByShipment)).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
                PendingTotal = pendingTotal,
                SettledTotal = settledTotal
            };
        }

        public async Task<CodRemittanceListItemResponse> MarkSettledAsync(
            string adminUserId,
            CodSettlementMarkSettledRequest request)
        {
            if (request.CodSettlementId <= 0)
                throw new InvalidOperationException(SystemMessage.CodSettlementIdRequired);
            if (string.IsNullOrWhiteSpace(request.PaymentReference))
                throw new InvalidOperationException(SystemMessage.CodSettlementPaymentRefRequired);

            var entity = await _codSettlementRepository.GetByIdAsync(request.CodSettlementId)
                ?? throw new KeyNotFoundException(SystemMessage.CodSettlementNotFound);

            if (entity.Status == SettlementStatus.Settled)
                throw new InvalidOperationException(SystemMessage.CodSettlementAlreadySettled);

            entity.Status = SettlementStatus.Settled;
            entity.SettlementDate = request.SettlementDate?.Date ?? DateTime.UtcNow.Date;
            entity.PaymentReference = request.PaymentReference.Trim();
            entity.Remark = string.IsNullOrWhiteSpace(request.Remark) ? null : request.Remark.Trim();
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = adminUserId;

            await _codSettlementRepository.UpdateAsync(entity);
            await _codSettlementRepository.SaveChangesAsync();

            var freightByShipment = await _shipmentChargesRepository
                .GetSellerChargesByShipmentIdsAsync([entity.ShipmentId]);

            return MapCod(entity, freightByShipment);
        }

        private static SettlementStatus? ParseSettlementStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status) || status.Equals("all", StringComparison.OrdinalIgnoreCase))
                return null;

            return status.Trim().ToLowerInvariant() switch
            {
                "pending" => SettlementStatus.Pending,
                "settled" => SettlementStatus.Settled,
                "failed" => SettlementStatus.Failed,
                _ => null
            };
        }

        private static CodRemittanceListItemResponse MapCod(
            CODSettlement c,
            IReadOnlyDictionary<long, decimal> freightByShipment)
        {
            var remittance = c.CollectedAmount > 0 ? c.CollectedAmount : c.CodAmount;
            freightByShipment.TryGetValue(c.ShipmentId, out var freightDeductions);

            return new CodRemittanceListItemResponse
            {
                RemittanceId = c.CodSettlementId,
                ShipmentId = c.ShipmentId,
                AwbNumber = c.Shipment?.AwbNumber,
                CodAmount = c.CodAmount,
                Status = c.Status.ToString(),
                StatusCode = (int)c.Status,
                PaymentDate = c.SettlementDate,
                FreightDeductions = freightDeductions,
                RemittanceAmount = remittance,
                ConvenienceFee = 0m,
                PaymentRef = c.Status == SettlementStatus.Settled
                    ? c.PaymentReference ?? $"COD-{c.CodSettlementId}"
                    : null,
                Remark = c.Remark
            };
        }
    }
}
