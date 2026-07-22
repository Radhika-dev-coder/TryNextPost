using TryNextPost.Application.DTO.Courier;
using TryNextPost.Application.DTO.Shipment;
using TryNextPost.Application.Helpers;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.IServices.Interface.Courier;
using TryNextPost.Application.IServices.Interface.IShipment;
using TryNextPost.Application.IServices.Interface.IRateCard;
using TryNextPost.Application.IServices.Interface.IWallet;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class.Shipment
{
    public class ShipmentService : IShipmentService
    {
        private readonly ISellerRepository _sellerRepository;
        private readonly ISellerContextService _sellerContextService;
        private readonly IOrderRepository _orderRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IShipmentRepository _shipmentRepository;
        private readonly INdrRepository _ndrRepository;
        private readonly ICourierRepository _courierRepository;
        private readonly ICourierAdapterFactory _courierAdapterFactory;
        private readonly IWalletService _walletService;
        private readonly IRateCalculationService _rateCalculationService;
        private readonly IShipmentChargesRepository _shipmentChargesRepository;

        public ShipmentService(
            ISellerRepository sellerRepository,
            ISellerContextService sellerContextService,
            IOrderRepository orderRepository,
            IAddressRepository addressRepository,
            IShipmentRepository shipmentRepository,
            INdrRepository ndrRepository,
            ICourierRepository courierRepository,
            ICourierAdapterFactory courierAdapterFactory,
            IWalletService walletService,
            IRateCalculationService rateCalculationService,
            IShipmentChargesRepository shipmentChargesRepository)
        {
            _sellerRepository = sellerRepository;
            _sellerContextService = sellerContextService;
            _orderRepository = orderRepository;
            _addressRepository = addressRepository;
            _shipmentRepository = shipmentRepository;
            _ndrRepository = ndrRepository;
            _courierRepository = courierRepository;
            _courierAdapterFactory = courierAdapterFactory;
            _walletService = walletService;
            _rateCalculationService = rateCalculationService;
            _shipmentChargesRepository = shipmentChargesRepository;
        }

        public async Task<GetShipmentRatesResponse> GetRatesAsync(
            long orderId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.ShipmentsCreate);
            var (order, seller) = await LoadOwnedOrderAsync(orderId, userId);
            EnsureOrderShippable(order);

            var warehouse = await ResolveWarehouseAddressAsync(order, seller);
            var rateRequest = BuildRateRequest(order, warehouse);
            var couriers = await _courierRepository.GetActiveCouriersAsync();
            var isCod = rateRequest.IsCod;

            var rates = new List<ShipmentRateOptionDto>();

            foreach (var courier in couriers)
            {
                var rateCardQuotes = await _rateCalculationService.GetRatesForCourierAsync(
                    courier.CourierId,
                    courier.CourierCode,
                    courier.CourierName,
                    rateRequest.OriginPincode,
                    rateRequest.DestinationPincode,
                    order.WeightGrams,
                    order.VolumetricWeightGrams,
                    isCod);

                if (rateCardQuotes.Count > 0)
                {
                    foreach (var quote in rateCardQuotes)
                    {
                        rates.Add(new ShipmentRateOptionDto
                        {
                            CourierId = courier.CourierId,
                            CourierCode = courier.CourierCode,
                            CourierName = courier.CourierName,
                            ServiceName = quote.ServiceName,
                            ServiceCode = quote.ServiceCode,
                            TotalCharge = quote.TotalCharge,
                            CodCharge = quote.CodCharge,
                            EstimatedDays = quote.EstimatedDays,
                            IsStub = false,
                            Message = $"Rate card ({quote.OriginZoneCode} → {quote.DestinationZoneCode})"
                        });
                    }

                    continue;
                }

                if (!_courierAdapterFactory.TryResolve(courier.CourierCode, out var adapter) || adapter is null)
                    continue;

                try
                {
                    var response = await adapter.GetRatesAsync(rateRequest, cancellationToken);
                    if (response?.Rates == null || response.Rates.Count == 0)
                        continue;

                    foreach (var option in response.Rates)
                    {
                        rates.Add(new ShipmentRateOptionDto
                        {
                            CourierId = courier.CourierId,
                            CourierCode = courier.CourierCode,
                            CourierName = courier.CourierName,
                            ServiceName = option.ServiceName,
                            ServiceCode = option.ServiceCode,
                            TotalCharge = option.TotalCharge,
                            CodCharge = option.CodCharge,
                            EstimatedDays = option.EstimatedDays,
                            IsStub = response.IsStub || option.IsStub,
                            Message = response.Message
                        });
                    }
                }
                catch (NotImplementedException)
                {
                    // Credentials configured but HTTP not wired yet — skip for rates list.
                }
            }

            return new GetShipmentRatesResponse
            {
                OrderId = order.OrderId,
                OrderRef = order.OrderRef,
                OriginPincode = rateRequest.OriginPincode,
                DestinationPincode = rateRequest.DestinationPincode,
                Rates = rates.OrderBy(r => r.TotalCharge).ToList()
            };
        }

        public async Task<ConfirmShipmentResponse> ConfirmShipmentAsync(
            ConfirmShipmentRequest request,
            string userId,
            CancellationToken cancellationToken = default)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.ShipmentsCreate);

            if (request.ChargeAmount <= 0)
                throw new InvalidOperationException(SystemMessage.ChargeAmountInvalid);

            if (!request.CourierId.HasValue && string.IsNullOrWhiteSpace(request.CourierCode))
                throw new InvalidOperationException(SystemMessage.CourierRequired);

            var (order, seller) = await LoadOwnedOrderAsync(request.OrderId, userId);
            EnsureOrderShippable(order);

            if (await _shipmentRepository.HasActiveShipmentAsync(order.OrderId))
                throw new InvalidOperationException(SystemMessage.ShipmentAlreadyExists);

            var courier = await ResolveCourierAsync(request.CourierId, request.CourierCode);
            if (!_courierAdapterFactory.TryResolve(courier.CourierCode, out var adapter) || adapter is null)
                throw new InvalidOperationException(SystemMessage.CourierNotSupported);

            var warehouse = await ResolveWarehouseAddressAsync(order, seller);
            var rateRequest = BuildRateRequest(order, warehouse);
            var rateQuote = await ResolveRateQuoteAsync(
                order,
                warehouse,
                courier,
                request.ServiceCode,
                cancellationToken);

            if (rateQuote != null)
            {
                if (Math.Abs(rateQuote.TotalCharge - request.ChargeAmount) > 0.01m)
                    throw new InvalidOperationException(SystemMessage.ChargeAmountMismatch);
            }
            else
            {
                await ValidateChargeAmountAsync(order, warehouse, courier, request, adapter, cancellationToken);
            }

            // Balance check before courier booking (avoid orphaned AWBs on insufficient funds).
            var wallet = await _walletService.GetSellerWalletBalanceAsync(userId);
            if (wallet.Balance < request.ChargeAmount)
                throw new InvalidOperationException(SystemMessage.WalletInsufficientBalance);

            var bookRequest = BuildBookRequest(order, warehouse, request.ServiceCode);

            CourierBookShipmentResponse bookResponse;
            try
            {
                bookResponse = await adapter.BookShipmentAsync(bookRequest, cancellationToken);
            }
            catch (NotImplementedException ex)
            {
                throw new InvalidOperationException(
                    $"{SystemMessage.ShipmentBookingFailed} {ex.Message}");
            }

            if (bookResponse == null || !bookResponse.Success || string.IsNullOrWhiteSpace(bookResponse.AwbNumber))
            {
                throw new InvalidOperationException(
                    bookResponse?.Message ?? SystemMessage.ShipmentBookingFailed);
            }

            var isReverse = order.OrderType == OrderTypeEnum.Reverse
                || order.OrderType == OrderTypeEnum.ReverseQC;

            var warehouseName = string.IsNullOrWhiteSpace(warehouse.Name)
                ? (warehouse.WarehouseName ?? "Warehouse")
                : warehouse.Name;
            var warehouseCountry = string.IsNullOrWhiteSpace(warehouse.Country) ? "India" : warehouse.Country;
            var customerCountry = string.IsNullOrWhiteSpace(order.ShippingCountry) ? "India" : order.ShippingCountry;

            var shipment = new Domain.Entities.Shipment
            {
                OrderId = order.OrderId,
                CourierId = courier.CourierId,
                AwbNumber = bookResponse.AwbNumber.Trim(),
                CourierReference = bookResponse.CourierReference,
                ServiceCode = request.ServiceCode,
                LabelUrl = bookResponse.LabelUrl,
                ChargedAmount = request.ChargeAmount,
                ShipmentType = MapShipmentType(order.OrderType),
                // Always store seller warehouse FK (return-to for reverse).
                PickupAddressId = warehouse.AddressId,
                // Delivery snapshot = physical delivery destination for this booking.
                DeliveryCustomerName = isReverse ? warehouseName : order.CustomerName,
                DeliveryMobile = isReverse ? warehouse.Mobile : order.CustomerMobile,
                DeliveryAddressLine1 = isReverse ? warehouse.AddressLine1 : order.ShippingAddressLine1,
                DeliveryAddressLine2 = isReverse ? warehouse.AddressLine2 : order.ShippingAddressLine2,
                DeliveryCity = isReverse ? warehouse.City : order.ShippingCity,
                DeliveryState = isReverse ? warehouse.State : order.ShippingState,
                DeliveryPincode = isReverse ? warehouse.Pincode : order.ShippingPincode,
                DeliveryCountry = isReverse ? warehouseCountry : customerCountry,
                Weight = order.WeightGrams,
                Length = order.LengthCm,
                Breadth = order.BreadthCm,
                Height = order.HeightCm,
                Status = ShipmentStatus.Booked,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            order.Status = OrderStatus.Confirmed;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = userId;

            await _shipmentRepository.AddAsync(shipment);
            await _orderRepository.UpdateAsync(order);
            await _shipmentRepository.SaveChangesAsync();

            var charges = BuildShipmentCharges(
                shipment.ShipmentId,
                rateQuote,
                rateRequest,
                order,
                request,
                userId);
            await _shipmentChargesRepository.AddAsync(charges);
            await _shipmentChargesRepository.SaveChangesAsync();

            var walletAfterDebit = await _walletService.DebitForShipmentAsync(
                userId,
                request.ChargeAmount,
                shipment.ShipmentId,
                shipment.AwbNumber,
                userId);

            await _shipmentRepository.AddTrackingAsync(new ShipmentTracking
            {
                ShipmentId = shipment.ShipmentId,
                Status = ShipmentStatus.Booked,
                StatusCode = "BOOKED",
                Location = string.Empty,
                Description = bookResponse.IsStub
                    ? "[STUB] Shipment booked (fake AWB)."
                    : "Shipment booked successfully.",
                EventTime = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
            await _shipmentRepository.SaveChangesAsync();

            return new ConfirmShipmentResponse
            {
                ShipmentId = shipment.ShipmentId,
                OrderId = order.OrderId,
                AwbNumber = shipment.AwbNumber,
                CourierId = courier.CourierId,
                CourierCode = courier.CourierCode,
                CourierName = courier.CourierName,
                ServiceCode = shipment.ServiceCode,
                Status = (int)shipment.Status,
                StatusName = shipment.Status.ToString(),
                ChargedAmount = shipment.ChargedAmount,
                WalletBalanceAfterDebit = walletAfterDebit.Balance,
                IsStub = bookResponse.IsStub,
                LabelUrl = shipment.LabelUrl,
                CourierReference = shipment.CourierReference,
                Message = bookResponse.Message ?? SystemMessage.ShipmentBookedSuccess
            };
        }

        public async Task<ShipmentLabelResponse> GetLabelAsync(
            long shipmentId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var shipment = await LoadOwnedShipmentAsync(shipmentId, userId);

            if (string.IsNullOrWhiteSpace(shipment.AwbNumber))
                throw new InvalidOperationException(SystemMessage.AwbRequired);

            if (!_courierAdapterFactory.TryResolve(shipment.Courier?.CourierCode, out var adapter) || adapter is null)
                throw new InvalidOperationException(SystemMessage.CourierNotSupported);

            CourierLabelResponse labelResponse;
            try
            {
                labelResponse = await adapter.GetLabelAsync(
                    new CourierLabelRequest { AwbNumber = shipment.AwbNumber },
                    cancellationToken);
            }
            catch (NotImplementedException ex)
            {
                throw new InvalidOperationException($"{SystemMessage.ShipmentLabelFailed} {ex.Message}");
            }

            if (labelResponse == null || !labelResponse.Success)
            {
                throw new InvalidOperationException(
                    labelResponse?.Message ?? SystemMessage.ShipmentLabelFailed);
            }

            if (!string.IsNullOrWhiteSpace(labelResponse.LabelUrl)
                && string.IsNullOrWhiteSpace(shipment.LabelUrl))
            {
                shipment.LabelUrl = labelResponse.LabelUrl;
                shipment.UpdatedAt = DateTime.UtcNow;
                shipment.UpdatedBy = userId;
                await _shipmentRepository.UpdateAsync(shipment);
                await _shipmentRepository.SaveChangesAsync();
            }

            return new ShipmentLabelResponse
            {
                ShipmentId = shipment.ShipmentId,
                AwbNumber = shipment.AwbNumber,
                LabelUrl = labelResponse.LabelUrl ?? shipment.LabelUrl,
                ContentType = labelResponse.ContentType,
                LabelBase64 = labelResponse.LabelContent == null
                    ? null
                    : Convert.ToBase64String(labelResponse.LabelContent),
                IsStub = labelResponse.IsStub,
                Message = labelResponse.Message ?? SystemMessage.ShipmentLabelFetchedSuccess
            };
        }

        public async Task<CancelShipmentResponse> CancelShipmentAsync(
            long shipmentId,
            CancelShipmentRequest request,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var shipment = await LoadOwnedShipmentAsync(shipmentId, userId);

            if (!ShipmentStatusTransitions.IsCancellable(shipment.Status))
                throw new InvalidOperationException(SystemMessage.ShipmentNotCancellable);

            ShipmentStatusTransitions.EnsureCanTransition(shipment.Status, ShipmentStatus.Cancelled);

            if (string.IsNullOrWhiteSpace(shipment.AwbNumber))
                throw new InvalidOperationException(SystemMessage.AwbRequired);

            if (!_courierAdapterFactory.TryResolve(shipment.Courier?.CourierCode, out var adapter) || adapter is null)
                throw new InvalidOperationException(SystemMessage.CourierNotSupported);

            CourierCancelResponse cancelResponse;
            try
            {
                cancelResponse = await adapter.CancelAsync(
                    new CourierCancelRequest
                    {
                        AwbNumber = shipment.AwbNumber,
                        Reason = request.Reason
                    },
                    cancellationToken);
            }
            catch (NotImplementedException ex)
            {
                throw new InvalidOperationException($"{SystemMessage.ShipmentCancelFailed} {ex.Message}");
            }

            if (cancelResponse == null || !cancelResponse.Success)
            {
                throw new InvalidOperationException(
                    cancelResponse?.Message ?? SystemMessage.ShipmentCancelFailed);
            }

            shipment.Status = ShipmentStatus.Cancelled;
            shipment.UpdatedAt = DateTime.UtcNow;
            shipment.UpdatedBy = userId;

            await _shipmentRepository.UpdateAsync(shipment);
            await _shipmentRepository.AddTrackingAsync(new ShipmentTracking
            {
                ShipmentId = shipment.ShipmentId,
                Status = ShipmentStatus.Cancelled,
                StatusCode = "CANCELLED",
                Location = string.Empty,
                Description = request.Reason
                    ?? (cancelResponse.IsStub
                        ? "[STUB] Shipment cancel acknowledged."
                        : "Shipment cancelled."),
                EventTime = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
            await _shipmentRepository.SaveChangesAsync();

            return new CancelShipmentResponse
            {
                ShipmentId = shipment.ShipmentId,
                AwbNumber = shipment.AwbNumber,
                Status = (int)shipment.Status,
                StatusName = shipment.Status.ToString(),
                IsStub = cancelResponse.IsStub,
                Message = cancelResponse.Message ?? SystemMessage.ShipmentCancelledSuccess
            };
        }

        public async Task<ShipmentTrackResponse> TrackShipmentAsync(
            long shipmentId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var shipment = await LoadOwnedShipmentAsync(shipmentId, userId);

            if (string.IsNullOrWhiteSpace(shipment.AwbNumber))
                throw new InvalidOperationException(SystemMessage.AwbRequired);

            if (!_courierAdapterFactory.TryResolve(shipment.Courier?.CourierCode, out var adapter) || adapter is null)
                throw new InvalidOperationException(SystemMessage.CourierNotSupported);

            CourierTrackResponse trackResponse;
            try
            {
                trackResponse = await adapter.TrackAsync(
                    new CourierTrackRequest { AwbNumber = shipment.AwbNumber },
                    cancellationToken);
            }
            catch (NotImplementedException ex)
            {
                throw new InvalidOperationException($"{SystemMessage.ShipmentTrackFailed} {ex.Message}");
            }

            if (trackResponse == null || !trackResponse.Success)
            {
                throw new InvalidOperationException(
                    trackResponse?.Message ?? SystemMessage.ShipmentTrackFailed);
            }

            // Soft-sync local status from courier current status when parseable + allowed.
            if (ShipmentStatusTransitions.TryParseStatus(trackResponse.CurrentStatus, out var mapped)
                && mapped != shipment.Status
                && ShipmentStatusTransitions.CanTransition(shipment.Status, mapped))
            {
                shipment.Status = mapped;
                shipment.UpdatedAt = DateTime.UtcNow;
                shipment.UpdatedBy = userId;
                await _shipmentRepository.UpdateAsync(shipment);
                await _shipmentRepository.SaveChangesAsync();
            }

            var localHistory = await _shipmentRepository.GetTrackingHistoryAsync(shipment.ShipmentId);
            var events = new List<ShipmentTrackEventDto>();

            events.AddRange(localHistory.Select(t => new ShipmentTrackEventDto
            {
                EventTime = t.EventTime,
                Status = t.Status.ToString(),
                StatusCode = t.StatusCode,
                Location = t.Location,
                Description = t.Description,
                FromLocalHistory = true
            }));

            if (trackResponse.Events != null)
            {
                foreach (var e in trackResponse.Events)
                {
                    events.Add(new ShipmentTrackEventDto
                    {
                        EventTime = e.EventTime,
                        Status = e.Status,
                        StatusCode = e.StatusCode,
                        Location = e.Location,
                        Description = e.Description,
                        FromLocalHistory = false
                    });
                }
            }

            return new ShipmentTrackResponse
            {
                ShipmentId = shipment.ShipmentId,
                AwbNumber = shipment.AwbNumber,
                Status = (int)shipment.Status,
                StatusName = shipment.Status.ToString(),
                CourierCurrentStatus = trackResponse.CurrentStatus,
                IsStub = trackResponse.IsStub,
                Message = trackResponse.Message ?? SystemMessage.ShipmentTrackedSuccess,
                Events = events.OrderByDescending(e => e.EventTime).ToList()
            };
        }

        public async Task<ShipmentTrackingWebhookResponse> ProcessTrackingWebhookAsync(
            ShipmentTrackingWebhookRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.AwbNumber))
                throw new InvalidOperationException(SystemMessage.TrackingWebhookInvalid);

            ShipmentStatus newStatus;
            if (request.StatusCode.HasValue
                && Enum.IsDefined(typeof(ShipmentStatus), request.StatusCode.Value))
            {
                newStatus = (ShipmentStatus)request.StatusCode.Value;
            }
            else if (!ShipmentStatusTransitions.TryParseStatus(request.Status, out newStatus))
            {
                throw new InvalidOperationException(SystemMessage.TrackingWebhookInvalid);
            }

            var shipment = await _shipmentRepository.GetByAwbAsync(request.AwbNumber.Trim());
            if (shipment == null)
                throw new InvalidOperationException(SystemMessage.ShipmentNotFound);

            if (shipment.Status != newStatus)
                ShipmentStatusTransitions.EnsureCanTransition(shipment.Status, newStatus);

            shipment.Status = newStatus;
            shipment.UpdatedAt = DateTime.UtcNow;
            shipment.UpdatedBy = "webhook";

            await _shipmentRepository.UpdateAsync(shipment);
            await _shipmentRepository.AddTrackingAsync(new ShipmentTracking
            {
                ShipmentId = shipment.ShipmentId,
                Status = newStatus,
                StatusCode = request.CourierStatusCode
                    ?? request.StatusCode?.ToString()
                    ?? newStatus.ToString().ToUpperInvariant(),
                Location = request.Location ?? string.Empty,
                Description = request.Description
                    ?? $"Webhook status update: {newStatus}",
                EventTime = request.EventTime ?? DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

            await ApplyNdrFromTrackingAsync(
                shipment,
                newStatus,
                request.Description ?? $"Webhook status update: {newStatus}");

            await _shipmentRepository.SaveChangesAsync();

            return new ShipmentTrackingWebhookResponse
            {
                ShipmentId = shipment.ShipmentId,
                AwbNumber = shipment.AwbNumber,
                Status = (int)shipment.Status,
                StatusName = shipment.Status.ToString(),
                Message = SystemMessage.TrackingWebhookAccepted
            };
        }

        /// <summary>
        /// Upsert / close NDR rows from courier tracking status (Phase-1).
        /// Exception → open ActionRequired (+Attempts); Delivered → Delivered; RTO → Rto.
        /// </summary>
        private async Task ApplyNdrFromTrackingAsync(
            Domain.Entities.Shipment shipment,
            ShipmentStatus newStatus,
            string reason)
        {
            if (newStatus == ShipmentStatus.Exception)
            {
                var open = await _ndrRepository.GetOpenByShipmentIdAsync(shipment.ShipmentId);
                if (open == null)
                {
                    await _ndrRepository.AddAsync(new NDR
                    {
                        ShipmentId = shipment.ShipmentId,
                        Reason = reason,
                        Attempts = 1,
                        Status = NdrStatus.ActionRequired,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "webhook"
                    });
                }
                else
                {
                    open.Attempts += 1;
                    open.Reason = reason;
                    open.Status = NdrStatus.ActionRequired;
                    open.UpdatedAt = DateTime.UtcNow;
                    open.UpdatedBy = "webhook";
                    await _ndrRepository.UpdateAsync(open);
                }

                return;
            }

            if (newStatus != ShipmentStatus.Delivered && newStatus != ShipmentStatus.RTO)
                return;

            var existing = await _ndrRepository.GetOpenByShipmentIdAsync(shipment.ShipmentId);
            if (existing == null)
                return;

            existing.Status = newStatus == ShipmentStatus.Delivered
                ? NdrStatus.Delivered
                : NdrStatus.Rto;
            existing.Reason = string.IsNullOrWhiteSpace(reason) ? existing.Reason : reason;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = "webhook";
            await _ndrRepository.UpdateAsync(existing);
        }

        public async Task<ShipmentListResponse> GetShipmentsAsync(string userId, ShipmentFilterRequest request)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.ShipmentsView);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);

            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);
            var statusFilter = ParseStatusTab(request.StatusTab);

            var shipments = await _shipmentRepository.GetBySellerFilteredAsync(
                seller.SellerId, statusFilter, page, pageSize, request.SearchQuery);
            var totalCount = await _shipmentRepository.GetBySellerFilteredCountAsync(
                seller.SellerId, statusFilter, request.SearchQuery);

            var tabCounts = new ShipmentTabCounts
            {
                All = await _shipmentRepository.GetCountBySellerAndStatusAsync(seller.SellerId, null),
                Booked = await _shipmentRepository.GetCountBySellerAndStatusAsync(seller.SellerId, ShipmentStatus.Booked),
                PendingPickup = await _shipmentRepository.GetCountBySellerAndStatusAsync(seller.SellerId, ShipmentStatus.PendingPickup),
                PickedUp = await _shipmentRepository.GetCountBySellerAndStatusAsync(seller.SellerId, ShipmentStatus.PickedUp),
                InTransit = await _shipmentRepository.GetCountBySellerAndStatusAsync(seller.SellerId, ShipmentStatus.InTransit),
                OutForDelivery = await _shipmentRepository.GetCountBySellerAndStatusAsync(seller.SellerId, ShipmentStatus.OutForDelivery),
                Delivered = await _shipmentRepository.GetCountBySellerAndStatusAsync(seller.SellerId, ShipmentStatus.Delivered),
                Rto = await _shipmentRepository.GetCountBySellerAndStatusAsync(seller.SellerId, ShipmentStatus.RTO),
                Exception = await _shipmentRepository.GetCountBySellerAndStatusAsync(seller.SellerId, ShipmentStatus.Exception),
                Cancelled = await _shipmentRepository.GetCountBySellerAndStatusAsync(seller.SellerId, ShipmentStatus.Cancelled)
            };

            return new ShipmentListResponse
            {
                Shipments = shipments.Select(MapToListItem).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TabCounts = tabCounts
            };
        }

        public async Task<ShipmentDetailResponse> GetShipmentByOrderIdAsync(long orderId, string userId)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.ShipmentsView);
            await LoadOwnedOrderAsync(orderId, userId);

            var shipment = await _shipmentRepository.GetByOrderIdAsync(orderId);
            if (shipment == null)
                throw new InvalidOperationException(SystemMessage.ShipmentNotFound);

            return new ShipmentDetailResponse
            {
                ShipmentId = shipment.ShipmentId,
                OrderId = shipment.OrderId,
                AwbNumber = shipment.AwbNumber,
                CourierId = shipment.CourierId,
                CourierCode = shipment.Courier?.CourierCode,
                CourierName = shipment.Courier?.CourierName,
                ServiceCode = shipment.ServiceCode,
                Status = (int)shipment.Status,
                StatusName = shipment.Status.ToString(),
                ShipmentType = (int)shipment.ShipmentType,
                ChargedAmount = shipment.ChargedAmount,
                LabelUrl = shipment.LabelUrl,
                CourierReference = shipment.CourierReference,
                Weight = shipment.Weight,
                Length = shipment.Length,
                Breadth = shipment.Breadth,
                Height = shipment.Height,
                DeliveryCustomerName = shipment.DeliveryCustomerName,
                DeliveryPincode = shipment.DeliveryPincode,
                DeliveryCity = shipment.DeliveryCity,
                CreatedAt = shipment.CreatedAt
            };
        }

        private async Task<(Domain.Entities.Order Order, Seller Seller)> LoadOwnedOrderAsync(long orderId, string userId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.IsActive == false)
                throw new InvalidOperationException(SystemMessage.OrderNotFound);

            var seller = await _sellerContextService.ResolveSellerAsync(userId);
            if (order.SellerId != seller.SellerId)
                throw new UnauthorizedAccessException(SystemMessage.Unauthorized);

            return (order, seller);
        }

        private async Task<Domain.Entities.Shipment> LoadOwnedShipmentAsync(long shipmentId, string userId)
        {
            var shipment = await _shipmentRepository.GetByIdAsync(shipmentId);
            if (shipment == null)
                throw new InvalidOperationException(SystemMessage.ShipmentNotFound);

            var seller = await _sellerContextService.ResolveSellerAsync(userId);
            if (shipment.Order == null || shipment.Order.SellerId != seller.SellerId)
                throw new UnauthorizedAccessException(SystemMessage.Unauthorized);

            return shipment;
        }

        private static void EnsureOrderShippable(Domain.Entities.Order order)
        {
            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException(SystemMessage.OrderNotShippable);
        }

        /// <summary>
        /// Resolves the seller warehouse / return-to address.
        /// Forward: requires order.PickupAddressId.
        /// Reverse / ReverseQC: order.PickupAddressId → seller.DefaultPickupAddressId → first active SellerPickup (soft heal).
        /// </summary>
        private async Task<Address> ResolveWarehouseAddressAsync(Domain.Entities.Order order, Seller seller)
        {
            var isReverse = order.OrderType == OrderTypeEnum.Reverse
                || order.OrderType == OrderTypeEnum.ReverseQC;

            long? warehouseId = order.PickupAddressId;

            if (!warehouseId.HasValue && isReverse)
                warehouseId = seller.DefaultPickupAddressId;

            // Soft fallback for Reverse QC "order 15" style: any active seller warehouse.
            if (!warehouseId.HasValue && isReverse)
            {
                var pickups = await _addressRepository.GetByUserIdAsync(
                    seller.UserId, AddressType.SellerPickup);
                var first = pickups.OrderBy(a => a.AddressId).FirstOrDefault();
                if (first != null)
                {
                    warehouseId = first.AddressId;

                    // Auto-heal default so future reverse orders work without manual SQL.
                    if (!seller.DefaultPickupAddressId.HasValue)
                    {
                        seller.DefaultPickupAddressId = first.AddressId;
                        seller.UpdatedAt = DateTime.UtcNow;
                        await _sellerRepository.UpdateAsync(seller);
                        await _sellerRepository.SaveChangesAsync();
                    }
                }
            }

            if (!warehouseId.HasValue)
            {
                throw new InvalidOperationException(
                    isReverse ? SystemMessage.ReturnWarehouseRequired : SystemMessage.PickupAddressRequired);
            }

            var warehouse = await _addressRepository.GetByIdAsync(warehouseId.Value);
            if (warehouse == null || warehouse.IsActive == false)
            {
                throw new InvalidOperationException(
                    isReverse ? SystemMessage.ReturnWarehouseRequired : SystemMessage.PickupAddressRequired);
            }

            return warehouse;
        }

        private async Task<Courier> ResolveCourierAsync(long? courierId, string? courierCode)
        {
            Courier? courier = null;

            if (courierId.HasValue && courierId.Value > 0)
                courier = await _courierRepository.GetByIdAsync(courierId.Value);

            if (courier == null && !string.IsNullOrWhiteSpace(courierCode))
                courier = await _courierRepository.GetByCodeAsync(courierCode);

            if (courier == null)
                throw new InvalidOperationException(SystemMessage.CourierNotFound);

            return courier;
        }

        private static CourierRateRequest BuildRateRequest(Domain.Entities.Order order, Address warehouse)
        {
            var isReverse = order.OrderType == OrderTypeEnum.Reverse
                || order.OrderType == OrderTypeEnum.ReverseQC;
            var isCod = !isReverse && order.PaymentMode == PaymentMode.COD;

            var origin = isReverse ? order.ShippingPincode : warehouse.Pincode;
            var destination = isReverse ? warehouse.Pincode : order.ShippingPincode;

            return new CourierRateRequest
            {
                OriginPincode = origin,
                DestinationPincode = destination,
                WeightKg = ToKg(GetChargeableWeightGrams(order)),
                LengthCm = order.LengthCm,
                BreadthCm = order.BreadthCm,
                HeightCm = order.HeightCm,
                IsCod = isCod,
                CodAmount = isCod
                    ? (order.CollectableAmount ?? order.FinalPayableAmount)
                    : null,
                PaymentMode = order.PaymentMode.ToString()
            };
        }

        private static CourierBookShipmentRequest BuildBookRequest(
            Domain.Entities.Order order,
            Address warehouse,
            string? serviceCode)
        {
            var isReverse = order.OrderType == OrderTypeEnum.Reverse
                || order.OrderType == OrderTypeEnum.ReverseQC;
            var isCod = !isReverse && order.PaymentMode == PaymentMode.COD;
            var productDescription = order.OrderItems != null && order.OrderItems.Count > 0
                ? string.Join(", ", order.OrderItems.Select(i => i.ProductName).Take(3))
                : "Goods";

            var warehouseName = string.IsNullOrWhiteSpace(warehouse.Name)
                ? (warehouse.WarehouseName ?? "Warehouse")
                : warehouse.Name;
            var warehouseCountry = string.IsNullOrWhiteSpace(warehouse.Country) ? "India" : warehouse.Country;
            var customerCountry = string.IsNullOrWhiteSpace(order.ShippingCountry) ? "India" : order.ShippingCountry;

            if (isReverse)
            {
                return new CourierBookShipmentRequest
                {
                    OrderRef = order.OrderRef,
                    ServiceCode = serviceCode,
                    PickupName = order.CustomerName,
                    PickupPhone = order.CustomerMobile,
                    PickupAddressLine1 = order.ShippingAddressLine1,
                    PickupAddressLine2 = order.ShippingAddressLine2,
                    PickupCity = order.ShippingCity,
                    PickupState = order.ShippingState,
                    PickupPincode = order.ShippingPincode,
                    PickupCountry = customerCountry,
                    DeliveryName = warehouseName,
                    DeliveryPhone = warehouse.Mobile,
                    DeliveryAddressLine1 = warehouse.AddressLine1,
                    DeliveryAddressLine2 = warehouse.AddressLine2,
                    DeliveryCity = warehouse.City,
                    DeliveryState = warehouse.State,
                    DeliveryPincode = warehouse.Pincode,
                    DeliveryCountry = warehouseCountry,
                    WeightKg = ToKg(GetChargeableWeightGrams(order)),
                    LengthCm = order.LengthCm,
                    BreadthCm = order.BreadthCm,
                    HeightCm = order.HeightCm,
                    IsCod = false,
                    CodAmount = null,
                    InvoiceValue = order.FinalPayableAmount,
                    ProductDescription = productDescription
                };
            }

            return new CourierBookShipmentRequest
            {
                OrderRef = order.OrderRef,
                ServiceCode = serviceCode,
                PickupName = warehouseName,
                PickupPhone = warehouse.Mobile,
                PickupAddressLine1 = warehouse.AddressLine1,
                PickupAddressLine2 = warehouse.AddressLine2,
                PickupCity = warehouse.City,
                PickupState = warehouse.State,
                PickupPincode = warehouse.Pincode,
                PickupCountry = warehouseCountry,
                DeliveryName = order.CustomerName,
                DeliveryPhone = order.CustomerMobile,
                DeliveryAddressLine1 = order.ShippingAddressLine1,
                DeliveryAddressLine2 = order.ShippingAddressLine2,
                DeliveryCity = order.ShippingCity,
                DeliveryState = order.ShippingState,
                DeliveryPincode = order.ShippingPincode,
                DeliveryCountry = customerCountry,
                WeightKg = ToKg(GetChargeableWeightGrams(order)),
                LengthCm = order.LengthCm,
                BreadthCm = order.BreadthCm,
                HeightCm = order.HeightCm,
                IsCod = isCod,
                CodAmount = isCod
                    ? (order.CollectableAmount ?? order.FinalPayableAmount)
                    : null,
                InvoiceValue = order.FinalPayableAmount,
                ProductDescription = productDescription
            };
        }

        private static ShipmentCharges BuildShipmentCharges(
            long shipmentId,
            DTO.RateCard.RateQuoteDto? rateQuote,
            CourierRateRequest rateRequest,
            Domain.Entities.Order order,
            ConfirmShipmentRequest request,
            string userId)
        {
            if (rateQuote != null)
            {
                return new ShipmentCharges
                {
                    ShipmentId = shipmentId,
                    SellerCharge = rateQuote.SellerCharge,
                    CourierCost = rateQuote.CourierCost,
                    Margin = rateQuote.Margin,
                    CodCharge = rateQuote.CodCharge,
                    ChargeableWeightGrams = rateQuote.ChargeableWeightGrams,
                    OriginZoneCode = rateQuote.OriginZoneCode,
                    DestinationZoneCode = rateQuote.DestinationZoneCode,
                    ServiceCode = request.ServiceCode,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };
            }

            var codCharge = rateRequest.IsCod ? 30m : 0m;
            var sellerCharge = request.ChargeAmount - codCharge;
            var courierCost = Math.Round(sellerCharge * 0.75m, 2);

            return new ShipmentCharges
            {
                ShipmentId = shipmentId,
                SellerCharge = sellerCharge,
                CourierCost = courierCost,
                Margin = sellerCharge - courierCost,
                CodCharge = codCharge,
                ChargeableWeightGrams = GetChargeableWeightGrams(order),
                ServiceCode = request.ServiceCode,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
        }

        private async Task<DTO.RateCard.RateQuoteDto?> ResolveRateQuoteAsync(
            Domain.Entities.Order order,
            Address warehouse,
            Courier courier,
            string? serviceCode,
            CancellationToken cancellationToken)
        {
            var rateRequest = BuildRateRequest(order, warehouse);
            var quote = await _rateCalculationService.GetRateForServiceAsync(
                courier.CourierId,
                courier.CourierCode,
                courier.CourierName,
                rateRequest.OriginPincode,
                rateRequest.DestinationPincode,
                order.WeightGrams,
                order.VolumetricWeightGrams,
                rateRequest.IsCod,
                serviceCode);

            return quote;
        }

        private async Task ValidateChargeAmountAsync(
            Domain.Entities.Order order,
            Address warehouse,
            Courier courier,
            ConfirmShipmentRequest request,
            ICourierAdapter adapter,
            CancellationToken cancellationToken)
        {
            var rateRequest = BuildRateRequest(order, warehouse);

            CourierRateResponse? rateResponse;
            try
            {
                rateResponse = await adapter.GetRatesAsync(rateRequest, cancellationToken);
            }
            catch (NotImplementedException)
            {
                return;
            }

            if (rateResponse?.Rates == null || rateResponse.Rates.Count == 0)
                return;

            var matched = string.IsNullOrWhiteSpace(request.ServiceCode)
                ? rateResponse.Rates.OrderBy(r => r.TotalCharge).FirstOrDefault()
                : rateResponse.Rates.FirstOrDefault(r =>
                    string.Equals(r.ServiceCode, request.ServiceCode, StringComparison.OrdinalIgnoreCase));

            if (matched == null)
                throw new InvalidOperationException(SystemMessage.ChargeAmountMismatch);

            if (Math.Abs(matched.TotalCharge - request.ChargeAmount) > 0.01m)
                throw new InvalidOperationException(SystemMessage.ChargeAmountMismatch);
        }

        private static ShipmentListItemResponse MapToListItem(Domain.Entities.Shipment shipment)
        {
            return new ShipmentListItemResponse
            {
                ShipmentId = shipment.ShipmentId,
                OrderId = shipment.OrderId,
                OrderRef = shipment.Order?.OrderRef,
                AwbNumber = shipment.AwbNumber,
                CourierId = shipment.CourierId,
                CourierCode = shipment.Courier?.CourierCode,
                CourierName = shipment.Courier?.CourierName,
                ServiceCode = shipment.ServiceCode,
                Status = (int)shipment.Status,
                StatusName = shipment.Status.ToString(),
                ShipmentType = (int)shipment.ShipmentType,
                ShipmentTypeName = shipment.ShipmentType.ToString(),
                ChargedAmount = shipment.ChargedAmount,
                DeliveryCustomerName = shipment.DeliveryCustomerName,
                DeliveryPincode = shipment.DeliveryPincode,
                DeliveryCity = shipment.DeliveryCity,
                CreatedAt = shipment.CreatedAt
            };
        }

        private static ShipmentStatus? ParseStatusTab(string? statusTab)
        {
            if (string.IsNullOrWhiteSpace(statusTab) || statusTab.Equals("all", StringComparison.OrdinalIgnoreCase))
                return null;

            var normalized = statusTab.Trim().Replace("-", "").Replace("_", "").Replace(" ", "");

            return normalized.ToLowerInvariant() switch
            {
                "booked" => ShipmentStatus.Booked,
                "pendingpickup" => ShipmentStatus.PendingPickup,
                "pickedup" or "picked" => ShipmentStatus.PickedUp,
                "intransit" => ShipmentStatus.InTransit,
                "outofordelivery" => ShipmentStatus.OutForDelivery,
                "delivered" => ShipmentStatus.Delivered,
                "rto" => ShipmentStatus.RTO,
                "reacheddestination" => ShipmentStatus.ReachedDestination,
                "exception" => ShipmentStatus.Exception,
                "cancelled" or "canceled" => ShipmentStatus.Cancelled,
                "bookingfailed" => ShipmentStatus.BookingFailed,
                "created" => ShipmentStatus.Created,
                _ => throw new InvalidOperationException(SystemMessage.InvalidShipmentStatusTab)
            };
        }

        private static ShipmentType MapShipmentType(OrderTypeEnum orderType)
        {
            return orderType == OrderTypeEnum.Forward
                ? ShipmentType.Forward
                : ShipmentType.Reverse;
        }

        private static decimal ToKg(decimal weightGrams)
        {
            return Math.Max(weightGrams / 1000m, 0.1m);
        }

        private static decimal GetChargeableWeightGrams(Domain.Entities.Order order)
        {
            var actual = order.WeightGrams > 0 ? order.WeightGrams : 500m;
            if (order.VolumetricWeightGrams > actual)
                return order.VolumetricWeightGrams;
            return actual;
        }
    }
}
