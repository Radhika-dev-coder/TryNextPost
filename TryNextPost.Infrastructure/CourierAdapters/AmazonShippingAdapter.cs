using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.AmazonDto;
using TryNextPost.Application.DTO.Courier;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.IServices.Interface.Courier;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Infrastructure.CourierAdapters
{
    public class AmazonShippingAdapter : ICourierAdapter
    {
        private readonly IAmazonShippingService _amazonShippingService;

        public AmazonShippingAdapter( IAmazonShippingService amazonShippingService)
        {
            _amazonShippingService = amazonShippingService;
        }

        public string CourierCode => "AMAZON";

        public async Task<CourierRateResponse> GetRatesAsync(CourierRateRequest request, CancellationToken cancellationToken = default)
        {

            var amazonRequest = new AmazonGetRatesRequest
            {
                ShipFrom = new AmazonAddress
                {
                    Name = "Test Seller",
                    AddressLine1 = "Test Warehouse",
                    StateOrRegion = "Telangana",
                    PostalCode = request.OriginPincode,
                    City = "Hyderabad",
                    CountryCode = "IN"
                },

                ShipTo = new AmazonAddress
                {
                    Name = "Test Customer",
                    AddressLine1 = "Test Address",
                    StateOrRegion = "Telangana",
                    PostalCode = request.DestinationPincode,
                    City = "Hyderabad",
                    CountryCode = "IN"
                },

                Packages =
       [
           new AmazonPackage
        {
            Dimensions = new AmazonDimensions
            {
                Length = request.LengthCm ?? 20,
                Width = request.BreadthCm ?? 15,
                Height = request.HeightCm ?? 10,
                Unit = "CENTIMETER"
            },

            Weight = new AmazonWeight
            {
                Value = request.WeightKg * 1000,
                Unit = "GRAM"
            },

            Items =
            [
                new AmazonItem
                {
                    Quantity = 1,
                    ItemIdentifier = "TRY-NEXT-ITEM",
                    Description = "Order Item",
                    IsHazmat = false,
                    Weight = new AmazonWeight
                    {
                        Value = request.WeightKg * 1000,
                        Unit = "GRAM"
                    }
                }
            ],

            InsuredValue = new AmazonMoney
            {
                Unit = "INR",
                Value = request.CodAmount ?? 0
            },

            PackageClientReferenceId =
                $"TRYNEXT-{Guid.NewGuid():N}"
        }
       ],

                ChannelDetails = new AmazonChannelDetails
                {
                    ChannelType = "EXTERNAL"
                },

                ServiceSelection = new AmazonServiceSelection
                {
                    ServiceId = ["SWA-IN-OA"]
                }
            };

            AmazonGetRatesResponse amazonResponse;

            try
            {
                amazonResponse =
                    await _amazonShippingService.GetRatesAsync(
                        amazonRequest,
                        cancellationToken);
            }
            catch (Exception ex)
            {
                // Credentials missing/invalid or Amazon API error should not
                // crash rate lookups for other couriers in the same request.
                return new CourierRateResponse
                {
                    Success = false,
                    IsStub = false,
                    CourierCode = CourierCode,
                    Message = $"Amazon Shipping rates unavailable: {ex.Message}"
                };
            }

            if (amazonResponse?.Payload?.Rates == null)
            {
                return new CourierRateResponse
                {
                    Success = false,
                    IsStub = false,
                    CourierCode = CourierCode,
                    Message = "Amazon Shipping returned no rate options."
                };
            }

            var result = new CourierRateResponse
            {
                Success = true,
                IsStub = false,
                CourierCode = CourierCode,
                Message = "Amazon Shipping rates fetched successfully."
            };

            foreach (var rate in amazonResponse.Payload.Rates)
            {
                var estimatedDays =
                    CalculateEstimatedDays(rate);

                result.Rates.Add(
                    new CourierRateOption
                    {
                        ServiceName = rate.ServiceName,
                        ServiceCode = rate.ServiceId,
                        RateId = rate.RateId,
                        RequestToken = amazonResponse.Payload.RequestToken,
                        TotalCharge = rate.TotalCharge.Value,
                        CodCharge = null,
                        EstimatedDays = estimatedDays,
                        IsStub = false
                    });
            }

            return result;
        }

        private static int CalculateEstimatedDays(
            AmazonRate rate)
        {
            // Amazon response currently gives delivery window.
            // We will improve this mapping in the next step.
            return 1;
        }

        public async Task<CourierBookShipmentResponse> BookShipmentAsync(CourierBookShipmentRequest request,
 CancellationToken cancellationToken = default)
        {
            var amazonRequest = new AmazonCreateShipmentRequest
            {
                ShipFrom = new AmazonAddress
                {
                    Name = request.PickupName,
                    AddressLine1 = request.PickupAddressLine1,
                    StateOrRegion = request.PickupState,
                    PostalCode = request.PickupPincode,
                    City = request.PickupCity,
                    CountryCode = MapCountryCode(request.PickupCountry),
                    PhoneNumber = request.PickupPhone
                },

                ShipTo = new AmazonAddress
                {
                    Name = request.DeliveryName,
                    AddressLine1 = request.DeliveryAddressLine1,
                    StateOrRegion = request.DeliveryState,
                    PostalCode = request.DeliveryPincode,
                    City = request.DeliveryCity,
                    CountryCode = MapCountryCode(request.DeliveryCountry),
                    PhoneNumber = request.DeliveryPhone
                },

                Packages =
                [
                    new AmazonPackage
            {
                Dimensions = new AmazonDimensions
                {
                    Length = request.LengthCm ?? 20,
                    Width = request.BreadthCm ?? 15,
                    Height = request.HeightCm ?? 10,
                    Unit = "CENTIMETER"
                },

                Weight = new AmazonWeight
                {
                    Value = request.WeightKg * 1000,
                    Unit = "GRAM"
                },

                Items =
                [
                    new AmazonItem
                    {
                        Quantity = 1,
                        ItemIdentifier = request.OrderRef,
                        Description = request.ProductDescription ?? "Goods",
                        IsHazmat = false,
                        Weight = new AmazonWeight
                        {
                            Value = request.WeightKg * 1000,
                            Unit = "GRAM"
                        }
                    }
                ],

                InsuredValue = new AmazonMoney
                {
                    Unit = "INR",
                    Value = request.InvoiceValue ?? 0
                },

                PackageClientReferenceId =
                    $"TRYNEXT-{request.OrderRef}"
            }
                ],

                ServiceId = request.ServiceCode,

                ChannelDetails = new AmazonChannelDetails
                {
                    ChannelType = "EXTERNAL"
                },

                ClientReferenceId = request.OrderRef

            };

            var amazonResponse =
                await _amazonShippingService.CreateShipmentAsync(
                    amazonRequest,
                    cancellationToken);

            var payload = amazonResponse?.Payload;

            if (payload == null)
            {
                return new CourierBookShipmentResponse
                {
                    Success = false,
                    IsStub = false,
                    CourierCode = CourierCode,
                    Message = "Amazon returned an empty shipment response."
                };
            }

            var awb =
                !string.IsNullOrWhiteSpace(payload.TrackingId)
                    ? payload.TrackingId
                    : payload.ShipmentId;

            if (string.IsNullOrWhiteSpace(awb))
            {
                return new CourierBookShipmentResponse
                {
                    Success = false,
                    IsStub = false,
                    CourierCode = CourierCode,
                    Message = "Amazon shipment created but AWB/tracking number was not returned."
                };
            }

            return new CourierBookShipmentResponse
            {
                Success = true,
                IsStub = false,
                CourierCode = CourierCode,
                AwbNumber = awb,
                CourierReference = payload.ShipmentId,
                LabelUrl = payload.LabelUrl,
                Message = "Amazon shipment booked successfully."
            };
        }

        private static string MapCountryCode(string? country)
        {
            if (string.IsNullOrWhiteSpace(country))
                return "IN";

            return country.Trim().ToUpperInvariant() switch
            {
                "INDIA" => "IN",
                "IN" => "IN",
                _ => country.Trim().ToUpperInvariant()
            };
        }


        public async Task<CourierLabelResponse> GetLabelAsync(CourierLabelRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.AwbNumber))
            {
                return new CourierLabelResponse
                {
                    Success = false,
                    IsStub = false,
                    Message = "AWB number is required."
                };
            }

            // For Amazon, the AWB/TrackingId is currently used
            // as the shipment identifier.
            var amazonRequest = new AmazonGetLabelRequest
            {
                ShipmentId = request.AwbNumber,
                TrackingId = request.AwbNumber,
                LabelFormat = "PDF"
            };

            var amazonResponse =
                await _amazonShippingService.GetLabelAsync(
                    amazonRequest,
                    cancellationToken);

            var payload = amazonResponse?.Payload;

            if (payload == null)
            {
                return new CourierLabelResponse
                {
                    Success = false,
                    IsStub = false,
                    CourierCode = CourierCode,
                    Message = "Amazon returned an empty label response."
                };
            }

            byte[]? labelContent = null;

            if (!string.IsNullOrWhiteSpace(payload.LabelContent))
            {
                try
                {
                    labelContent = Convert.FromBase64String(
                        payload.LabelContent);
                }
                catch (FormatException)
                {
                    // LabelContent was not valid Base64.
                    // LabelUrl may still be available.
                    labelContent = null;
                }
            }

            if (string.IsNullOrWhiteSpace(payload.LabelUrl)
                && labelContent == null)
            {
                return new CourierLabelResponse
                {
                    Success = false,
                    IsStub = false,
                    CourierCode = CourierCode,
                    Message = "Amazon did not return a label URL or label content."
                };
            }

            return new CourierLabelResponse
            {
                Success = true,
                IsStub = false,
                CourierCode = CourierCode,
                LabelUrl = payload.LabelUrl,
                LabelContent = labelContent,
                ContentType = "application/pdf",
                Message = "Amazon shipment label fetched successfully."
            };
        }

        public async Task<CourierCancelResponse> CancelAsync(
         CourierCancelRequest request,
         CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.AwbNumber))
            {
                return new CourierCancelResponse
                {
                    Success = false,
                    IsStub = false,
                    CourierCode = CourierCode,
                    Message = "AWB number is required."
                };
            }

            var amazonRequest = new AmazonCancelShipmentRequest
            {
                // Current generic contract gives us AWB.
                // Amazon cancellation will use this as ShipmentId
                // until a separate Amazon ShipmentId is persisted.
                ShipmentId = request.AwbNumber,
                Reason = request.Reason
            };

            var amazonResponse =
                await _amazonShippingService.CancelShipmentAsync(
                    amazonRequest,
                    cancellationToken);

            var payload = amazonResponse?.Payload;

            if (payload == null)
            {
                return new CourierCancelResponse
                {
                    Success = false,
                    IsStub = false,
                    CourierCode = CourierCode,
                    Message = "Amazon returned an empty cancellation response."
                };
            }

            return new CourierCancelResponse
            {
                Success = true,
                IsStub = false,
                CourierCode = CourierCode,
                Message = !string.IsNullOrWhiteSpace(payload.Message)
                    ? payload.Message
                    : "Amazon shipment cancelled successfully."
            };
        }

        public async Task<CourierTrackResponse> TrackAsync(
      CourierTrackRequest request,
      CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.AwbNumber))
            {
                return new CourierTrackResponse
                {
                    Success = false,
                    IsStub = false,
                    CourierCode = CourierCode,
                    Message = "AWB number is required."
                };
            }

            var amazonRequest = new AmazonTrackShipmentRequest
            {
                ShipmentId = request.AwbNumber,
                TrackingId = request.AwbNumber
            };

            var amazonResponse =
                await _amazonShippingService.TrackShipmentAsync(
                    amazonRequest,
                    cancellationToken);

            var payload = amazonResponse?.Payload;

            if (payload == null)
            {
                return new CourierTrackResponse
                {
                    Success = false,
                    IsStub = false,
                    CourierCode = CourierCode,
                    Message = "Amazon returned an empty tracking response."
                };
            }

            var events = new List<CourierTrackEvent>();

            if (payload.Events != null)
            {
                foreach (var item in payload.Events)
                {
                    events.Add(new CourierTrackEvent
                    {
                        Status = item.Status ?? string.Empty,
                        StatusCode = item.StatusCode,
                        Location = item.Location ?? string.Empty,
                        Description = item.Description ?? string.Empty,
                        EventTime = item.EventTime ?? DateTime.UtcNow
                    });
                }
            }

            return new CourierTrackResponse
            {
                Success = true,
                IsStub = false,
                CourierCode = CourierCode,
                CurrentStatus = payload.Status ?? string.Empty,
                Events = events,
                Message = "Amazon shipment tracking fetched successfully."
            };
        }

        public Task<bool> IsServiceableAsync(string pickupPincode, string deliveryPincode, OrderTypeEnum orderType, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<CourierBookShipmentResponse> BookShipmentAsync(CourierShipmentRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RequestNdrReAttemptAsync(string awbNumber, string actionType, string remarks, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}