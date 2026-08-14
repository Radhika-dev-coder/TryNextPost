using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Application.DTO.Courier;
using TryNextPost.Application.DTO.Courier.XpressBees;
using TryNextPost.Domain.Entities;
using TryNextPost.Application.IServices.Class;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.CourierAdapters;
using TryNextPost.Infrastructure.CourierAdapters.Common;
using TryNextPost.Infrastructure.CourierAdapters.CourierConstant;

namespace TryNextPost.Infrastructure.CourierAdapters
{
    public sealed class XpressbeesAdapter : CourierAdapterBase
    {
        private readonly XpressbeesSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ICourierPickupLocationService _pickupLocationService;
        private readonly IAddressRepository _addressRepository;
        // private readonly IOrderRepository _orderRepository;


        public XpressbeesAdapter(
            IOptions<CourierSettings> options,
            ILogger<XpressbeesAdapter> logger,
            ICourierPickupLocationService pickupLocationService,
            IAddressRepository addressRepository,
            IOrderRepository orderRepository,
            HttpClient httpClient)
            : base(logger, orderRepository)
        {
            _settings = options.Value.Xpressbees;
            _httpClient = httpClient;
            _pickupLocationService = pickupLocationService;
            _addressRepository = addressRepository;
        }

        public override string CourierCode => CourierCodes.Xpressbees;

        //protected override XpressbeesSettings Settings => _settings;

        protected override bool IsConfigured =>
            _settings.Enabled
            && !string.IsNullOrWhiteSpace(_settings.TokenUrl)
            && !string.IsNullOrWhiteSpace(_settings.ForwardUrl)
            && !string.IsNullOrWhiteSpace(_settings.ApiKey)
            && !string.IsNullOrWhiteSpace(_settings.ApiSecret)
            && !string.IsNullOrWhiteSpace(_settings.SecretKey)
            && !string.IsNullOrWhiteSpace(_settings.XBKey);

        protected override async Task<CourierBookShipmentResponse> BookShipmentInternalAsync(
            CourierShipmentRequest request,
            CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetForShipmentAsync(request.OrderId, cancellationToken);

            if (order == null)
            {
                throw new InvalidOperationException(
                    $"Order not found for OrderId {request.OrderId}.");
            }

            var pickupAddress = await _addressRepository.GetByIdAsync(request.AddressId, cancellationToken);

            if (pickupAddress == null)
            {
                throw new InvalidOperationException(
                    $"Pickup address not found for AddressId {request.AddressId}.");
            }
            // Step 1: Generate authentication token
            var token = await GenerateTokenAsync();


            // Step 2: Check pickup serviceability
            var pickupResponse = await CheckServiceabilityAsync(
                token,
                order.OrderType,
                true,
                cancellationToken);


            // Step 3: Check delivery serviceability
            var deliveryResponse = await CheckServiceabilityAsync(
                token,
                 order.OrderType,
                false,
                cancellationToken);


            // Step 4: Validate pickup and delivery pincodes
            bool pickupOk = CourierValidationHelper.IsPincodeServiceable(
                pickupResponse.ServicablePincodeDetails
                    .Select(x => x.Pincode),
                pickupAddress.Pincode);

            bool deliveryOk = CourierValidationHelper.IsPincodeServiceable(
                deliveryResponse.ServicablePincodeDetails
                    .Select(x => x.Pincode),
                order.ShippingPincode);

            if (!pickupOk)
            {
                throw new InvalidOperationException(
                    $"Pickup pincode {pickupAddress.Pincode} is not serviceable.");
            }

            if (!deliveryOk)
            {
                throw new InvalidOperationException(
                    $"Delivery pincode {order.ShippingPincode} is not serviceable.");
            }


            // Step 5: Get AWB number

            // STAGING ONLY:
            // XpressBees Stage AWB Generation API is currently unavailable.
            // XpressBees provided AWBs for staging manifest testing.
            // PRODUCTION: Remove this hardcoded AWB and enable AWB generation API.

            var awbBatch = await GenerateAwbBatchAsync(
                order.PaymentMode == PaymentMode.COD,
                cancellationToken);

            var awbSeries = await GetAwbNumberGeneratedSeriesAsync(
                awbBatch.BatchID!,
                cancellationToken);

            if (awbSeries.AWBNoSeries == null ||
                awbSeries.AWBNoSeries.Count == 0)
            {
                throw new InvalidOperationException(
                    "XpressBees did not return any AWB number.");
            }

            var awb = awbSeries.AWBNoSeries[0];

            //// STAGING ONLY
            // var awb = "1540235000001";


            // Step 6: Get CourierId using CourierCode
            var courierId = await _pickupLocationService.GetCourierIdAsync(
                CourierCode,
                cancellationToken);

            if (courierId == null)
            {
                throw new InvalidOperationException(
                    $"Courier not found for code '{CourierCode}'.");
            }


            // Step 7: Get pickup address
            long addressId = request.AddressId;


            // Step 8: Get or create courier pickup location
            var pickupLocation =
                await _pickupLocationService.GetOrCreateAsync(
                    addressId,
                    courierId.Value, CourierCode,
                    cancellationToken);



            // Step 9: Build XpressBees manifest request
            var manifestRequest = BuildManifestRequest(
                order,
                pickupAddress,
                awb,
                pickupLocation.LocationCode);


            // Step 10: Add authentication token
            //_httpClient.DefaultRequestHeaders.Authorization =
            //    new AuthenticationHeaderValue(
            //        "Bearer",
            //        token);
            _httpClient.DefaultRequestHeaders.Remove("token");
            _httpClient.DefaultRequestHeaders.Add("token", token);

            _httpClient.DefaultRequestHeaders.Remove("versionnumber");
            _httpClient.DefaultRequestHeaders.Add("versionnumber", "v1");


            // Step 11: Serialize manifest request
            var manifestJson = JsonSerializer.Serialize(
                manifestRequest);


            // Step 12: Call XpressBees Manifest API
            var response = await _httpClient.PostAsync(
                _settings.ForwardUrl,
                new StringContent(
                    manifestJson,
                    Encoding.UTF8,
                    "application/json"),
                cancellationToken);


            // Step 13: Read API response
            var responseJson =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);


            // Step 14: Validate HTTP response
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"XpressBees Manifest API failed. " +
                    $"Response: {responseJson}");
            }


            // Step 15: Deserialize XpressBees response
            var manifestResponse =
                JsonSerializer.Deserialize<XpressBeesManifestResponse>(
                    responseJson);

            if (manifestResponse == null)
            {
                throw new InvalidOperationException(
                    "Invalid response received from XpressBees Manifest API.");
            }


            // Step 16: Validate XpressBees response
            if (manifestResponse.ReturnCode == 100 &&
                string.Equals(
                    manifestResponse.ReturnMessage,
                    "successful",
                    StringComparison.OrdinalIgnoreCase))
            {
                // Successful manifest
            }
            else if (manifestResponse.ReturnCode == 100 &&
                     string.Equals(
                         manifestResponse.ReturnMessage,
                         "AirWayBillNO Already exists",
                         StringComparison.OrdinalIgnoreCase))
            {
                return new CourierBookShipmentResponse
                {
                    Success = false,
                    IsStub = false,
                    CourierCode = CourierCode,
                    AwbNumber = null,
                    CourierReference = null,
                    LabelUrl = null,
                    Message = $"XpressBees AWB already exists. AWB: {awb}"
                };
            }
            else
            {
                return new CourierBookShipmentResponse
                {
                    Success = false,
                    IsStub = false,
                    CourierCode = CourierCode,
                    AwbNumber = null,
                    CourierReference = null,
                    LabelUrl = null,
                    Message =
                        $"XpressBees Manifest failed. " +
                        $"ReturnCode: {manifestResponse.ReturnCode}, " +
                        $"Message: {manifestResponse.ReturnMessage}"
                };
            }


            // Step 17: Return successful response
            return new CourierBookShipmentResponse
            {
                Success = true,
                CourierCode = CourierCode,
                AwbNumber = awb,
                CourierReference = order.OrderRef,
                Message = responseJson
            };
        }


        private async Task<XpressBeesServiceabilityResponse> CheckServiceabilityAsync(
    string token,
    OrderTypeEnum orderType,
    bool isPickup,
    CancellationToken cancellationToken)
        {
            var (businessFlow, businessService) =
                GetBusinessConfiguration(
                    orderType,
                    isPickup);

            var body = new XpressBeesServiceabilityRequest
            {
                BusinessUnit = _settings.BusinessUnit!,
                BusinessFlow = businessFlow,
                BusinessService = businessService
            };

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                _settings.ServiceabilityUrl);

            httpRequest.Headers.Add("token", token);
            httpRequest.Headers.Add("versionnumber", "v1");

            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "XpressBees Serviceability API failed. Status:{Status} Response:{Response}",
                    response.StatusCode,
                    json);

                throw new InvalidOperationException(
                    $"Serviceability API HTTP Error ({(int)response.StatusCode}) : {json}");
            }

            var result = JsonSerializer.Deserialize<XpressBeesServiceabilityResponse>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result == null)
            {
                throw new InvalidOperationException(
                    "Unable to deserialize XpressBees Serviceability response.");
            }

            switch (result.ReturnCode)
            {
                case 100:
                    return result;

                case 101:
                    throw new InvalidOperationException(
                        "XpressBees token expired or invalid.");

                case 103:
                    throw new InvalidOperationException(
                        $"Validation failed : {result.ReturnMessage}");

                case 104:
                    throw new InvalidOperationException(
                        "XpressBees operation failed. Please retry.");

                default:
                    throw new InvalidOperationException(
                        $"Unexpected XpressBees response. ReturnCode={result.ReturnCode}, Message={result.ReturnMessage}");
            }
        }

        //------For Testing-------------
        public async Task<string> GenerateTokenTestAsync()
        {
            return await GenerateTokenAsync();
        }

        public async Task<XpressBeesServiceabilityResponse> CheckServiceabilityTestAsync(
            OrderTypeEnum orderType,
            bool isPickup)
        {
            var token = await GenerateTokenAsync();

            return await CheckServiceabilityAsync(
                token,
                orderType,
                isPickup,
                CancellationToken.None);
        }
        public async Task<CourierBookShipmentResponse> BookShipmentTestAsync(
    CourierShipmentRequest request)
        {
            return await BookShipmentInternalAsync(
                request,
                CancellationToken.None);
        }
        //-------------------------------------

        private async Task<string> GenerateTokenAsync()
        {
            var body = new
            {
                username = _settings.ApiKey,
                password = _settings.ApiSecret,
                secretkey = _settings.SecretKey
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                _settings.TokenUrl,
                content);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Token API failed. Response : {json}");
            }
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                throw new Exception(error.GetString());
            }

            if (!doc.RootElement.TryGetProperty("token", out var tokenElement))
            {
                throw new Exception("Token not found in XpressBees response.");
            }

            return tokenElement.GetString()!;
        }

        private (string BusinessFlow, string BusinessService) GetBusinessConfiguration(
                OrderTypeEnum orderType,
                bool isPickup)
        {
            return orderType switch
            {
                OrderTypeEnum.Forward =>
                    ("Forward", isPickup ? "PickUp" : "Delivery"),

                OrderTypeEnum.Reverse =>
                    ("Reverse", isPickup ? "PickUp" : "Delivery"),

                OrderTypeEnum.ReverseQC =>
                    ("Reverse", isPickup ? "PickUp" : "Delivery"),

                _ => throw new InvalidOperationException("Invalid Order Type.")
            };
        }


        private async Task<XpressBeesAwbGenerationResponse> GenerateAwbBatchAsync(
    bool isCod,
    CancellationToken cancellationToken)
        {
            var request = new XpressBeesAwbGenerationRequest
            {
                BusinessUnit = "ECOM",
                ServiceType = "FORWARD",
                DeliveryType = isCod ? "COD" : "PREPAID"
            };

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                _settings.AwbGenerationUrl);

            httpRequest.Headers.Add("XBKey", _settings.XBKey);

            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

            var json = await response.Content.ReadAsStringAsync(
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"XpressBees AWB Generation API failed. Response: {json}");
            }

            var result =
                JsonSerializer.Deserialize<XpressBeesAwbGenerationResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (result == null)
            {
                throw new InvalidOperationException(
                    "Unable to deserialize XpressBees AWB Generation response.");
            }

            if (result.ReturnCode != 100)
            {
                throw new InvalidOperationException(
                    $"XpressBees AWB Generation failed: {result.ReturnMessage}");
            }

            if (string.IsNullOrWhiteSpace(result.BatchID))
            {
                throw new InvalidOperationException(
                    "XpressBees AWB Generation succeeded but BatchID was not returned.");
            }

            return result;
        }

        private async Task<XpressBeesGetAwbSeriesResponse> GetAwbNumberGeneratedSeriesAsync(
    string batchId,
    CancellationToken cancellationToken)
        {
            var request = new XpressBeesGetAwbSeriesRequest
            {
                BusinessUnit = "ECOM",
                ServiceType = "FORWARD",
                BatchID = batchId
            };

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                _settings.GetAwbSeriesUrl);

            httpRequest.Headers.Add("XBKey", _settings.XBKey);

            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

            var json = await response.Content.ReadAsStringAsync(
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"XpressBees Get AWB Series API failed. Response: {json}");
            }

            var result =
                JsonSerializer.Deserialize<XpressBeesGetAwbSeriesResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (result == null)
            {
                throw new InvalidOperationException(
                    "Unable to deserialize XpressBees Get AWB Series response.");
            }

            if (result.ReturnCode != 100)
            {
                throw new InvalidOperationException(
                    $"XpressBees Get AWB Series failed: {result.ReturnMessage}");
            }

            return result;
        }

        private XpressBeesManifestRequest BuildManifestRequest(
            Order order,
            Address pickupAddress,
            string awb,
            string pickupLocationCode)
        {
            return new XpressBeesManifestRequest
            {
                // AWB
                AirWayBillNO = awb,

                // Account
                BusinessAccountName = _settings.AccountCode!,

                // Order
                OrderNo = order.OrderRef,
                SubOrderNo = order.OrderRef,

                // Payment
                OrderType = order.PaymentMode == PaymentMode.COD
                    ? "COD"
                    : "PrePaid",

                CollectibleAmount = order.PaymentMode == PaymentMode.COD
                        ? (order.CollectableAmount ?? 0m).ToString("0.00")
                        : "0",
                DeclaredValue = order.FinalPayableAmount.ToString("0.00"),

                // Shipment
                PickupType = "Vendor",
                Quantity = "1",
                ServiceType = "SD",

                // =====================================================
                // DELIVERY = CUSTOMER
                // =====================================================

                DropDetails = new XpressBeesDropDetails
                {
                    Addresses =
            {
                new XpressBeesAddress
                {
                    Address = string.Join(", ", new[]
                    {
                        order.ShippingAddressLine1,
                        order.ShippingAddressLine2
                    }
                    .Where(x => !string.IsNullOrWhiteSpace(x))),

                    City = order.ShippingCity,
                    State = order.ShippingState,
                    PinCode = order.ShippingPincode,

                    Name = order.CustomerName,

                    Type = "Primary"
                }
            },

                    ContactDetails =
            {
                new XpressBeesContactDetails
                {
                    PhoneNo = order.CustomerMobile,
                    Type = "Primary"
                }
            }
                },

                // =====================================================
                // PICKUP = SELLER / WAREHOUSE
                // =====================================================

                PickupDetails = new XpressBeesPickupDetails
                {
                    Addresses =
            {
                new XpressBeesAddress
                {
                    Address = string.Join(", ", new[]
                    {
                        pickupAddress.AddressLine1,
                        pickupAddress.AddressLine2
                    }
                    .Where(x => !string.IsNullOrWhiteSpace(x))),

                    City = pickupAddress.City,
                    State = pickupAddress.State,
                    PinCode = pickupAddress.Pincode,

                    Name = pickupAddress.Name,

                    Type = "Primary"
                }
            },

                    ContactDetails =
            {
                new XpressBeesContactDetails
                {
                    PhoneNo = pickupAddress.Mobile,
                    Type = "Primary"
                }
            },

                    PickupVendorCode = pickupLocationCode
                },

                // =====================================================
                // RTO = PICKUP / SELLER ADDRESS
                // =====================================================

                RTODetails = new XpressBeesRtoDetails
                {
                    Addresses =
            {
                new XpressBeesAddress
                {
                    Address = string.Join(", ", new[]
                    {
                        pickupAddress.AddressLine1,
                        pickupAddress.AddressLine2
                    }
                    .Where(x => !string.IsNullOrWhiteSpace(x))),

                    City = pickupAddress.City,
                    State = pickupAddress.State,
                    PinCode = pickupAddress.Pincode,

                    Name = pickupAddress.Name,

                    Type = "Primary"
                }
            },

                    ContactDetails =
            {
                new XpressBeesContactDetails
                {
                    PhoneNo = pickupAddress.Mobile,
                    Type = "Primary"
                }
            }
                },

                // Manifest
                ManifestID = order.OrderRef,

                IsEssential = false,
                IsSecondaryPacking = false,

                // =====================================================
                // PACKAGE DETAILS
                // =====================================================

                PackageDetails = new XpressBeesPackageDetails
                {
                    Dimensions = new XpressBeesDimensions
                    {
                        Height = order.HeightCm.ToString("0.##"),
                        Length = order.LengthCm.ToString("0.##"),
                        Width = order.BreadthCm.ToString("0.##")
                    },

                    Weight = new XpressBeesWeight
                    {
                        BillableWeight =
                            (order.WeightGrams / 1000m).ToString("0.##"),

                        PhyWeight =
                            (order.WeightGrams / 1000m).ToString("0.##"),

                        VolWeight =
                            (order.VolumetricWeightGrams / 1000m).ToString("0.##")
                    }
                }
            };
        }
    }
}