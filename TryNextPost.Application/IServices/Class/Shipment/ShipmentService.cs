using Microsoft.Extensions.Logging;
using TryNextPost.Application.DTO.Courier;
using TryNextPost.Application.DTO.Courier.XpressBees;
using TryNextPost.Application.DTO.Ndr;
using TryNextPost.Application.DTO.Shipment;
using TryNextPost.Application.Helpers;
using TryNextPost.Application.IServices.Class.RateCard;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.IServices.Interface.Courier;
using TryNextPost.Application.IServices.Interface.IRateCard;
using TryNextPost.Application.IServices.Interface.IShipment;
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
        private readonly IProductWeightFreezeRepository _productWeightFreezeRepository;
        private readonly ILogger<ShipmentService> _logger;


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
            IShipmentChargesRepository shipmentChargesRepository,
            IProductWeightFreezeRepository productWeightFreezeRepository,
            ILogger<ShipmentService> logger)
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
            _productWeightFreezeRepository = productWeightFreezeRepository;
            _logger = logger;
        }



        public async Task<GetShipmentRatesResponse> GetRatesAsync(long orderId,  string userId, CancellationToken cancellationToken = default)
        {
            await _sellerContextService.EnsurePermissionAsync(
                userId,
                EmployeePermissionCode.ShipmentsCreate);

            var (order, seller) = await LoadOwnedOrderAsync(orderId, userId);

            EnsureOrderShippable(order);

            await ApplyWeightFreezeIfApplicableAsync(order);

            var warehouse = await ResolveWarehouseAddressAsync(order, seller);

            var rateRequest = BuildRateRequest(order, warehouse);

            var couriers = await _courierRepository.GetActiveCouriersAsync();

            var rates = new List<ShipmentRateOptionDto>();

            foreach (var courier in couriers)
            {
                bool rateFetched = false;

                if (courier.SupportsServiceabilityApi)
                {
                    if (_courierAdapterFactory.TryResolve(courier.CourierCode, out var adapter) && adapter != null)
                    {
                        bool isServiceable = false;

                        try
                        {
                            isServiceable = await adapter.IsServiceableAsync(
                                rateRequest.OriginPincode,
                                rateRequest.DestinationPincode,
                                order.OrderType,
                                cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Live serviceability check crashed for courier {CourierCode}. Falling back to default routing.", courier.CourierCode);
                            isServiceable = true;
                        }

                        if (!isServiceable)
                            continue;
                    }
                }

                // =========================================================
                // LIVE API
                // =========================================================
                if (courier.SupportsRateApi)
                {
                    if (_courierAdapterFactory.TryResolve(courier.CourierCode, out var adapter)
                        && adapter != null)
                    {
                        try
                        {
                            var adapterRequest = BuildRateRequest(order, warehouse, courier);

                            var response = await adapter.GetRatesAsync(
                                adapterRequest,
                                cancellationToken);

                            if (response.Success &&
                                response.Rates != null &&
                                response.Rates.Any())
                            {
                                foreach (var option in response.Rates)
                                {
                                    rates.Add(CreateShipmentRateOption(
                                        courier,
                                        option.ServiceName,
                                        option.ServiceCode,
                                        option.TotalCharge,
                                        option.CodCharge,
                                        option.EstimatedDays,
                                        response.IsStub || option.IsStub,
                                        response.Message));
                                }

                                rateFetched = true;
                            }
                        }
                        catch (NotImplementedException ex)
                        {
                            _logger.LogWarning( ex, "Rate API is not implemented for courier {CourierCode}", courier.CourierCode);
                        }
                        catch (Exception ex)
                        {
                           
                            _logger.LogError(ex, "Live rate fetch failed");
                            throw;
                        }
                    }
                }

                // Live API se rate mil gaya

                if (rateFetched)
                    continue;


                // RATE CARD

                var rateCardQuotes = await _rateCalculationService.GetRatesForCourierAsync(
                    courier.CourierId,
                    courier.CourierCode,
                    courier.CourierName,
                    rateRequest.OriginPincode,
                    rateRequest.DestinationPincode,
                    order.WeightGrams,
                    order.VolumetricWeightGrams,
                    rateRequest.IsCod,
                    courier.CodChargeType,
                    courier.CodChargeValue,
                    rateRequest.CodAmount,
                    courier.SupportsCOD,
                    courier.HasManualRateCard);

                foreach (var quote in rateCardQuotes)
                {
                    var rateSource =
                        quote.FromRateCard
                            ? $"Rate Card ({quote.OriginZoneCode} → {quote.DestinationZoneCode})"
                            : "Fixed Courier Rate";

                    rates.Add(CreateShipmentRateOption(
                        courier,
                        quote.ServiceName,
                        quote.ServiceCode,
                        quote.TotalCharge,
                        quote.CodCharge,
                        quote.EstimatedDays,
                        quote.FromRateCard,
                        rateSource));
                }
            }

            return new GetShipmentRatesResponse
            {
                OrderId = order.OrderId,
                OrderRef = order.OrderRef,
                OriginPincode = rateRequest.OriginPincode,
                DestinationPincode = rateRequest.DestinationPincode,
                Rates = rates.OrderBy(x => x.TotalCharge).ToList()
            };
        }

        private static ShipmentRateOptionDto CreateShipmentRateOption(Courier courier, string serviceName, string? serviceCode,
            decimal totalCharge, decimal? codCharge, int estimatedDays, bool isStub, string? message)
        {
            return new ShipmentRateOptionDto
            {
                CourierId = courier.CourierId,
                CourierCode = courier.CourierCode,
                CourierName = courier.CourierName,

                ServiceName = serviceName,
                ServiceCode = serviceCode,

                TotalCharge = totalCharge,
                CodCharge = codCharge ?? 0,

                EstimatedDays = estimatedDays,

                IsStub = isStub,

                Message = message
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
            await ApplyWeightFreezeIfApplicableAsync(order);

            if (await _shipmentRepository.HasActiveShipmentAsync(order.OrderId))
                throw new InvalidOperationException(SystemMessage.ShipmentAlreadyExists);

            var courier = await ResolveCourierAsync(request.CourierId, request.CourierCode);
            if (!_courierAdapterFactory.TryResolve(courier.CourierCode, out var adapter) || adapter is null)
                throw new InvalidOperationException(SystemMessage.CourierNotSupported);

            var warehouse = await ResolveWarehouseAddressAsync(order, seller);
            var rateRequest = BuildRateRequest(order, warehouse, courier);
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
                _logger.LogWarning("Rate Quote entity lookup returned null for Courier {CourierCode}. Proceeding to direct request amount verification path.", courier.CourierCode);
                await ValidateChargeAmountAsync(order, warehouse, courier, request, adapter, cancellationToken);
            }

            // Balance check before courier booking (avoid orphaned AWBs on insufficient funds).
            var wallet = await _walletService.GetSellerWalletBalanceAsync(userId);
            if (wallet.Balance < request.ChargeAmount)
                throw new InvalidOperationException(SystemMessage.WalletInsufficientBalance);

            //var bookRequest = BuildBookRequest(order, warehouse, request.ServiceCode);
            var bookRequest = new CourierShipmentRequest
            {
                OrderId = order.OrderId,
                AddressId = warehouse.AddressId,
                CourierId = courier.CourierId,
                ServiceCode = request.ServiceCode,
                ServiceType = request.ServiceCode
            };

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
                userId,
                courier);
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

            CourierLabelResponse? labelResponse = null;

            if (_courierAdapterFactory.TryResolve(shipment.Courier?.CourierCode, out var adapter) && adapter is not null)
            {
                try
                {
                    labelResponse = await adapter.GetLabelAsync(
                        new CourierLabelRequest { AwbNumber = shipment.AwbNumber },
                        cancellationToken);
                }
                catch (NotImplementedException)
                {
                    labelResponse = null;
                }
            }

            if (labelResponse != null && labelResponse.Success)
            {
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

            // Fallback: stored URL or local stub text label (no courier credentials required).
            if (!string.IsNullOrWhiteSpace(shipment.LabelUrl))
            {
                return new ShipmentLabelResponse
                {
                    ShipmentId = shipment.ShipmentId,
                    AwbNumber = shipment.AwbNumber,
                    LabelUrl = shipment.LabelUrl,
                    ContentType = "text/html",
                    IsStub = true,
                    Message = SystemMessage.ShipmentLabelFetchedSuccess
                };
            }

            var stubText = $"[STUB LABEL] AWB:{shipment.AwbNumber} Courier:{shipment.Courier?.CourierCode}";
            return new ShipmentLabelResponse
            {
                ShipmentId = shipment.ShipmentId,
                AwbNumber = shipment.AwbNumber,
                LabelUrl = shipment.LabelUrl,
                ContentType = "text/plain",
                LabelBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(stubText)),
                IsStub = true,
                Message = "[STUB] Local label — courier label API unavailable."
            };
        }

        public async Task<CancelShipmentResponse> CancelShipmentAsync(
            long shipmentId,
            CancelShipmentRequest request,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var shipment = await LoadOwnedShipmentAsync(shipmentId, userId);

            // Idempotent: already cancelled → ensure refund exists, return success.
            if (shipment.Status == ShipmentStatus.Cancelled)
            {
                var wallet = await RefundShipmentChargeAsync(shipment, userId);
                return new CancelShipmentResponse
                {
                    ShipmentId = shipment.ShipmentId,
                    AwbNumber = shipment.AwbNumber,
                    Status = (int)shipment.Status,
                    StatusName = shipment.Status.ToString(),
                    RefundedAmount = shipment.ChargedAmount,
                    WalletBalanceAfterRefund = wallet.Balance,
                    AlreadyCancelled = true,
                    IsStub = true,
                    Message = SystemMessage.ShipmentAlreadyCancelled
                };
            }

            if (!ShipmentStatusTransitions.IsCancellable(shipment.Status))
                throw new InvalidOperationException(SystemMessage.ShipmentNotCancellable);

            ShipmentStatusTransitions.EnsureCanTransition(shipment.Status, ShipmentStatus.Cancelled);

            if (string.IsNullOrWhiteSpace(shipment.AwbNumber))
                throw new InvalidOperationException(SystemMessage.AwbRequired);

            var cancelResponse = await TryCancelWithCourierAsync(shipment, request.Reason, cancellationToken);

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

            // Allow re-booking: revert order to Pending when it was Confirmed by booking.
            if (shipment.Order != null && shipment.Order.Status == OrderStatus.Confirmed)
            {
                shipment.Order.Status = OrderStatus.Pending;
                shipment.Order.UpdatedAt = DateTime.UtcNow;
                shipment.Order.UpdatedBy = userId;
                await _orderRepository.UpdateAsync(shipment.Order);
            }

            await _shipmentRepository.SaveChangesAsync();

            var walletAfterRefund = await RefundShipmentChargeAsync(shipment, userId);

            return new CancelShipmentResponse
            {
                ShipmentId = shipment.ShipmentId,
                AwbNumber = shipment.AwbNumber,
                Status = (int)shipment.Status,
                StatusName = shipment.Status.ToString(),
                RefundedAmount = shipment.ChargedAmount,
                WalletBalanceAfterRefund = walletAfterRefund.Balance,
                AlreadyCancelled = false,
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

            // Optional courier poll — never fail if local history exists (no real API required).
            CourierTrackResponse? trackResponse = null;
            if (_courierAdapterFactory.TryResolve(shipment.Courier?.CourierCode, out var adapter) && adapter is not null)
            {
                try
                {
                    trackResponse = await adapter.TrackAsync(
                        new CourierTrackRequest { AwbNumber = shipment.AwbNumber },
                        cancellationToken);
                }
                catch (NotImplementedException)
                {
                    trackResponse = null;
                }
                catch (Exception)
                {
                    // Keep local history as source of truth when courier is unavailable.
                    trackResponse = null;
                }
            }

            if (trackResponse != null && trackResponse.Success)
            {
                // Soft-sync only from real (non-stub) courier status.
                if (!trackResponse.IsStub
                    && ShipmentStatusTransitions.TryParseStatus(trackResponse.CurrentStatus, out var mapped)
                    && mapped != shipment.Status
                    && ShipmentStatusTransitions.CanTransition(shipment.Status, mapped))
                {
                    shipment.Status = mapped;
                    shipment.UpdatedAt = DateTime.UtcNow;
                    shipment.UpdatedBy = userId;
                    await _shipmentRepository.UpdateAsync(shipment);
                    await _shipmentRepository.SaveChangesAsync();
                }

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
            }

            if (events.Count == 0 && (trackResponse == null || !trackResponse.Success))
                throw new InvalidOperationException(SystemMessage.ShipmentTrackFailed);

            return new ShipmentTrackResponse
            {
                ShipmentId = shipment.ShipmentId,
                AwbNumber = shipment.AwbNumber,
                Status = (int)shipment.Status,
                StatusName = shipment.Status.ToString(),
                CourierCurrentStatus = trackResponse?.CurrentStatus ?? shipment.Status.ToString(),
                IsStub = trackResponse?.IsStub ?? true,
                Message = trackResponse?.Message ?? SystemMessage.ShipmentTrackedSuccess,
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

            if (shipment.Status == newStatus)
            {
                return new ShipmentTrackingWebhookResponse
                {
                    ShipmentId = shipment.ShipmentId,
                    AwbNumber = shipment.AwbNumber,
                    Status = (int)shipment.Status,
                    StatusName = shipment.Status.ToString(),
                    Message = "Duplicate status ignored"
                };
            }

            ShipmentStatusTransitions.EnsureCanTransition(shipment.Status, newStatus);

            shipment.Status = newStatus;
            shipment.UpdatedAt = DateTime.UtcNow;
            shipment.UpdatedBy = "webhook";

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

            await ApplyNdrFromTrackingAsync(shipment,newStatus,request.Description ?? $"Webhook status update: {newStatus}");

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

            if (newStatus != ShipmentStatus.Delivered && !ShipmentStatusTransitions.IsRtoStatus(newStatus))
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

            var isRto = request.StatusTab?.Equals("rto", StringComparison.OrdinalIgnoreCase) == true;
            var statusFilter = isRto ? null : ParseStatusTab(request.StatusTab);

            var orderCategory = request.OrderCategory;

            var shipments = await _shipmentRepository.GetBySellerFilteredAsync(
                seller.SellerId,
                statusFilter,
                isRto,
                page,
                pageSize,
                request.SearchQuery,
                orderCategory);

            var totalCount = await _shipmentRepository.GetBySellerFilteredCountAsync(
                seller.SellerId,
                statusFilter,
                isRto,
                request.SearchQuery,
                orderCategory);

            var statusCounts = await _shipmentRepository.GetStatusCountsBySellerAsync(
                seller.SellerId,
                orderCategory);
            var tabCounts = new ShipmentTabCounts
            {
                All = statusCounts.Values.Sum(),
                Booked = statusCounts.GetValueOrDefault(ShipmentStatus.Booked),
                PendingPickup = statusCounts.GetValueOrDefault(ShipmentStatus.PendingPickup),
                PickedUp = statusCounts.GetValueOrDefault(ShipmentStatus.PickedUp),
                InTransit = statusCounts.GetValueOrDefault(ShipmentStatus.InTransit),
                OutForDelivery = statusCounts.GetValueOrDefault(ShipmentStatus.OutForDelivery),
                Delivered = statusCounts.GetValueOrDefault(ShipmentStatus.Delivered),

                Rto = statusCounts
                    .Where(x => ShipmentStatusTransitions.IsRtoStatus(x.Key))
                    .Sum(x => x.Value),

                Exception = statusCounts.GetValueOrDefault(ShipmentStatus.Exception),
                Cancelled = statusCounts.GetValueOrDefault(ShipmentStatus.Cancelled)
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

        private static IQueryable<TryNextPost.Domain.Entities.Shipment> ApplyStatusFilter(IQueryable<TryNextPost.Domain.Entities.Shipment> query, string? statusTab)
        {
            if (statusTab?.Equals("rto", StringComparison.OrdinalIgnoreCase) == true)
                return query.Where(s => ShipmentStatusTransitions.IsRtoStatus(s.Status));

            var status = ParseStatusTab(statusTab);

            if (status.HasValue)
                return query.Where(s => s.Status == status.Value);

            return query;
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

        private async Task<DTO.Wallet.WalletBalanceResponse> RefundShipmentChargeAsync(
            Domain.Entities.Shipment shipment,
            string userId)
        {
            return await _walletService.CreditForShipmentRefundAsync(
                userId,
                shipment.ChargedAmount,
                shipment.ShipmentId,
                shipment.AwbNumber,
                userId);
        }

        private async Task<CourierCancelResponse> TryCancelWithCourierAsync(
            Domain.Entities.Shipment shipment,
            string? reason,
            CancellationToken cancellationToken)
        {
            if (!_courierAdapterFactory.TryResolve(shipment.Courier?.CourierCode, out var adapter) || adapter is null)
            {
                return new CourierCancelResponse
                {
                    Success = true,
                    IsStub = true,
                    CourierCode = shipment.Courier?.CourierCode ?? string.Empty,
                    Message = "[STUB] Local cancel — courier adapter not registered."
                };
            }

            try
            {
                var cancelResponse = await adapter.CancelAsync(
                    new CourierCancelRequest
                    {
                        AwbNumber = shipment.AwbNumber!,
                        Reason = reason
                    },
                    cancellationToken);

                if (cancelResponse == null || !cancelResponse.Success)
                {
                    throw new InvalidOperationException(
                        cancelResponse?.Message ?? SystemMessage.ShipmentCancelFailed);
                }

                return cancelResponse;
            }
            catch (NotImplementedException)
            {
                // Credentials configured but HTTP not wired — still cancel locally.
                return new CourierCancelResponse
                {
                    Success = true,
                    IsStub = true,
                    CourierCode = shipment.Courier?.CourierCode ?? string.Empty,
                    Message = "[STUB] Local cancel — courier cancel API not implemented yet."
                };
            }
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

        private static CourierRateRequest BuildRateRequest(
            Domain.Entities.Order order,
            Address warehouse,
            Courier? courier = null)
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
                PaymentMode = order.PaymentMode.ToString(),
                CodChargeType = courier?.CodChargeType ?? CodChargeType.Flat,
                CodChargeValue = courier?.CodChargeValue ?? 0m,
                TotalQuantity = order.OrderItems != null ? order.OrderItems.Sum(x => x.Qty) : 1,
                SupportsCod = courier?.SupportsCOD ?? true
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
                    ProductDescription = productDescription,
                    OrderType = order.OrderType,
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
            string userId,
            Courier? courier = null)
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

            var codCharge = courier != null
                ? RateCalculationService.ResolveCodCharge(
                    rateRequest.IsCod,
                    courier.SupportsCOD,
                    courier.CodChargeType,
                    courier.CodChargeValue,
                    rateRequest.CodAmount)
                : 0m;
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
                serviceCode,
                courier.CodChargeType,
                courier.CodChargeValue,
                rateRequest.CodAmount,
                courier.SupportsCOD);

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
            var rateRequest = BuildRateRequest(order, warehouse, courier);

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
                "outfordelivery" => ShipmentStatus.OutForDelivery,
                "delivered" => ShipmentStatus.Delivered,
                "rto" => null,
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

        /// <summary>
        /// Nimbus-like: when order items match an Accepted + AutoApply freeze (by ProductId/SKU),
        /// override in-memory package weight/dims for rate + book. Unfrozen/Rejected never apply.
        /// Does not persist order changes.
        /// </summary>
        private async Task ApplyWeightFreezeIfApplicableAsync(Domain.Entities.Order order)
        {
            if (order.OrderItems == null || order.OrderItems.Count == 0)
                return;

            var productKeys = order.OrderItems
                .Select(i => i.Sku)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (productKeys.Count == 0)
                return;

            var freezes = await _productWeightFreezeRepository.GetApplicableAcceptedAsync(
                order.SellerId,
                productKeys);

            if (freezes.Count == 0)
                return;

            ProductWeightFreeze? MatchFreeze(OrderItem item)
            {
                if (string.IsNullOrWhiteSpace(item.Sku))
                    return null;

                var key = item.Sku.Trim();
                return freezes.FirstOrDefault(f =>
                    (!string.IsNullOrWhiteSpace(f.Sku)
                     && string.Equals(f.Sku.Trim(), key, StringComparison.OrdinalIgnoreCase))
                    || string.Equals(f.ProductId.Trim(), key, StringComparison.OrdinalIgnoreCase));
            }

            decimal frozenWeight = 0;
            var matchedAny = false;
            ProductWeightFreeze? primaryFreeze = null;

            foreach (var item in order.OrderItems)
            {
                var freeze = MatchFreeze(item);
                if (freeze == null)
                    continue;

                matchedAny = true;
                primaryFreeze ??= freeze;
                var qty = item.Qty > 0 ? item.Qty : 1;
                frozenWeight += freeze.WeightGrams * qty;
            }

            if (!matchedAny || frozenWeight <= 0)
                return;

            order.WeightGrams = frozenWeight;

            if (primaryFreeze != null
                && primaryFreeze.LengthCm > 0
                && primaryFreeze.BreadthCm > 0
                && primaryFreeze.HeightCm > 0)
            {
                order.LengthCm = primaryFreeze.LengthCm;
                order.BreadthCm = primaryFreeze.BreadthCm;
                order.HeightCm = primaryFreeze.HeightCm;
                order.VolumetricWeightGrams =
                    (primaryFreeze.LengthCm * primaryFreeze.BreadthCm * primaryFreeze.HeightCm) / 5000m * 1000m;
            }
        }

        private static decimal GetChargeableWeightGrams(Domain.Entities.Order order)
        {
            var actual = order.WeightGrams > 0 ? order.WeightGrams : 500m;
            if (order.VolumetricWeightGrams > actual)
                return order.VolumetricWeightGrams;
            return actual;
        }

        // Layer Location: TryNextPost.Application / Services/ShipmentService.cs

        public async Task<NdrActionResponse> ProcessNdrActionAsync(
            NdrActionRequest request,
            string userId,
            CancellationToken cancellationToken = default)
        {
            // 1. Fetch data using pure Repository instead of direct DbContext
            var ndrLog = await _ndrRepository.GetNdrWithShipmentAndCourierAsync(request.NdrId, cancellationToken);

            if (ndrLog == null)
                throw new KeyNotFoundException("The requested NDR record is missing from data nodes.");

            var shipment = ndrLog.Shipment;
            if (shipment == null || string.IsNullOrWhiteSpace(shipment.AwbNumber))
                throw new InvalidOperationException("No valid shipment or AWB reference linked to this NDR.");

            // 2. Resolve adapter factory pipeline to fire live action to the courier server
            if (!_courierAdapterFactory.TryResolve(shipment.Courier?.CourierCode, out var adapter) || adapter == null)
            {
                throw new InvalidOperationException("Unable to resolve dynamic courier adapter for transmission.");
            }

            // Calling the courier adapter abstraction method
            bool courierServerAcknowledged = await adapter.RequestNdrReAttemptAsync(
                shipment.AwbNumber,
                request.ActionType,
                request.Remarks ?? "Action taken via Aggregator Seller Desk",
                cancellationToken);

            if (!courierServerAcknowledged)
            {
                return new NdrActionResponse
                {
                    Success = false,
                    Message = "Courier partner server rejected or failed to process the NDR instruction callback."
                };
            }

            // 3. Database State Synchronization using explicit repository commands
            if (request.ActionType.Equals("RE-ATTEMPT", StringComparison.OrdinalIgnoreCase))
            {
                ndrLog.Status = NdrStatus.ActionRequested; // State 2
                ndrLog.Action = "RE-ATTEMPT";
                ndrLog.NextAttemptDate = request.NextAttemptDate?? DateTime.UtcNow.AddDays(1);
                ndrLog.Attempts += 1;
            }
            else if (request.ActionType.Equals("RETURN_TO_ORIGIN", StringComparison.OrdinalIgnoreCase))
            {
                ndrLog.Status = NdrStatus.Rto; // State 4
                ndrLog.Action = "RTO";

                // Create an explicit fresh row track context entry into the RTO Master Table via NDR repository helper
                var rtoEntry = new RTO
                {
                    ShipmentId = shipment.ShipmentId,
                    Reason = ndrLog.Reason,
                    Status = RtoStatus.Initiated, // State 1
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };
                await _ndrRepository.AddRtoAsync(rtoEntry, cancellationToken);

                // Update Shipment table root status to RTO state indicators
                shipment.Status = ShipmentStatus.RTOInitiated;
                await _shipmentRepository.UpdateAsync(shipment);
            }

            ndrLog.Remarks = request.Remarks;
            ndrLog.UpdatedAt = DateTime.UtcNow;
            ndrLog.UpdatedBy = userId;

            // Use concrete repositories definitions instead of raw DbContext actions
            await _ndrRepository.UpdateAsync(ndrLog);
            await _shipmentRepository.SaveChangesAsync(); // Unit of Work trigger via shipment repository saving node

            return new NdrActionResponse
            {
                Success = true,
                Message = $"NDR instruction '{request.ActionType}' synchronized and saved permanently.",
                UpdatedStatusName = ndrLog.Status.ToString()
            };
        }

    }
}
