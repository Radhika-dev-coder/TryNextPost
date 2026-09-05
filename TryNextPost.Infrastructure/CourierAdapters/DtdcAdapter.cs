using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Application.DTO.Courier;
using TryNextPost.Application.DTO.Courier.Dtdc;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Infrastructure.CourierAdapters
{
    public sealed class DtdcAdapter : CourierAdapterBase
    {
        private readonly DtdcSettings _settings;
        private readonly HttpClient _httpClient;

        public DtdcAdapter(
            IOptions<CourierSettings> options,
            ILogger<DtdcAdapter> logger,
            IOrderRepository orderRepository,
            HttpClient httpClient)
            : base(logger, orderRepository)
        {
            _settings = options.Value.Dtdc;
            _httpClient = httpClient;
        }

        public override string CourierCode => CourierCodes.Dtdc;

        protected override bool IsConfigured =>
            _settings.Enabled &&
            !string.IsNullOrWhiteSpace(_settings.BookingUrl) &&
            !string.IsNullOrWhiteSpace(_settings.ApiKey) &&
            !string.IsNullOrWhiteSpace(_settings.AccountCode);



        protected override async Task<CourierBookShipmentResponse> BookShipmentInternalAsync(
           CourierShipmentRequest request,
           CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetForShipmentAsync(request.OrderId, cancellationToken);

            if (order == null)
            {
                throw new InvalidOperationException($"Order not found for OrderId {request.OrderId}.");
            }
            if (order.PickupAddress == null)
            {
                throw new InvalidOperationException($"Pickup address not found for OrderId {request.OrderId}.");
            }

            string currentWeightKg = (order.WeightGrams / 1000m).ToString("0.00");


            int totalItemsCount = order.OrderItems != null && order.OrderItems.Any()
                ? order.OrderItems.Sum(x => x.Qty)
                : 1;

            var dtdcPiecesList = new List<DtdcPiecesDetail>();

            if (order.OrderItems != null && order.OrderItems.Any())
            {
                foreach (var item in order.OrderItems)
                {
                    dtdcPiecesList.Add(new DtdcPiecesDetail
                    {
                        Description = !string.IsNullOrWhiteSpace(item.ProductName) ? item.ProductName : "E-Commerce Package Items",
                        DeclaredValue = (item.Price * item.Qty).ToString("0.00"),
                        Weight = (order.WeightGrams / 1000m / totalItemsCount).ToString("0.00"), // Distributed item average load weight
                        Length = order.LengthCm.ToString("0.0"),
                        Width = order.BreadthCm.ToString("0.0"),
                        Height = order.HeightCm.ToString("0.0")
                    });
                }
            }
            else
            {
                dtdcPiecesList.Add(new DtdcPiecesDetail
                {
                    Description = "General Goods Shipment Package",
                    DeclaredValue = order.FinalPayableAmount.ToString("0.00"),
                    Weight = currentWeightKg,
                    Length = order.LengthCm.ToString("0.0"),
                    Width = order.BreadthCm.ToString("0.0"),
                    Height = order.HeightCm.ToString("0.0")
                });
            }

            // REMOVED: var singlePieceElement is completely deleted from here to avoid duplicate bugs!

            var dtdcRequest = new DtdcBookingRequest
            {
                Consignments = new List<DtdcConsignment>
                {
                    new DtdcConsignment
                    {
                        CustomerCode = _settings.AccountCode,
                        ServiceTypeId = "B2C PRIORITY",
                        LoadType = "NON-DOCUMENT",
                        ConsignmentType = "Forward",
                        DimensionUnit = "cm",

                        Length = order.LengthCm.ToString("0.0"),
                        Width = order.BreadthCm.ToString("0.0"),
                        Height = order.HeightCm.ToString("0.0"),

                        WeightUnit = "kg",
                        Weight = currentWeightKg,

                        // =========================================================================
                        // 2. FIXED: Dynamically mapped fields from real calculated system parameters
                        // =========================================================================
                        NumPieces = totalItemsCount.ToString(),
                        PiecesDetail = dtdcPiecesList, // Linked the real items collection list loops!

                        CustomerReferenceNumber = order.OrderRef,
                        DeclaredValue = order.FinalPayableAmount.ToString("0.00"),
                        IsRiskSurchargeApplicable = "false",
                        
                        // FIXED: Mapped to official static placeholder for general ECOMMERCE CONTENT (99)
                        CommodityId = "99",

                        CodCollectionMode = order.PaymentMode == PaymentMode.COD ? "CASH" : "",
                        CodAmount = order.PaymentMode == PaymentMode.COD
                            ? (order.CollectableAmount ?? order.FinalPayableAmount).ToString("0.00")
                            : "",

                        OriginDetails = new DtdcOriginDetails
                        {
                            Name = order.PickupAddress.Name,
                            Phone = order.PickupAddress.Mobile,
                            AddressLine1 = order.PickupAddress.AddressLine1,
                            AddressLine2 = order.PickupAddress.AddressLine2 ?? "",
                            Pincode = order.PickupAddress.Pincode,
                            City = order.PickupAddress.City,
                            State = order.PickupAddress.State,
                            Email = order.PickupAddress.Email
                        },

                        DestinationDetails = new DtdcDestinationDetails
                        {
                            Name = order.CustomerName,
                            Phone = order.CustomerMobile,
                            AlternatePhone = order.CustomerMobile,
                            AddressLine1 = order.ShippingAddressLine1,
                            AddressLine2 = order.ShippingAddressLine2 ?? "",
                            City = order.ShippingCity,
                            State = order.ShippingState,
                            Pincode = order.ShippingPincode,
                            Email = ""
                        }
                    }
                }
            };

            var dtdcResponse = await CreateShipmentAsync(dtdcRequest, cancellationToken);

            if (dtdcResponse == null || dtdcResponse.Status != "OK" || dtdcResponse.Data == null || !dtdcResponse.Data.Any())
            {
                return new CourierBookShipmentResponse
                {
                    Success = false,
                    CourierCode = CourierCode,
                    Message = "DTDC API returned an invalid response structure or HTTP Error."
                };
            }

            var consignmentResult = dtdcResponse.Data.First();

            if (!consignmentResult.Success || string.IsNullOrWhiteSpace(consignmentResult.ReferenceNumber))
            {
                var liveErrorReason = !string.IsNullOrWhiteSpace(consignmentResult.ErrorDesc)
                    ? consignmentResult.ErrorDesc
                    : (!string.IsNullOrWhiteSpace(consignmentResult.ErrorMessage) ? consignmentResult.ErrorMessage : "DTDC internal validation failed to generate AWB.");

                return new CourierBookShipmentResponse
                {
                    Success = false,
                    CourierCode = CourierCode,
                    Message = $"{liveErrorReason} (Ref ID: {consignmentResult.CustomerReferenceNumber})"
                };
            }

            return new CourierBookShipmentResponse
            {
                Success = true,
                CourierCode = CourierCode,
                AwbNumber = consignmentResult.ReferenceNumber,
                CourierReference = consignmentResult.CourierPartnerReferenceNumber ?? "",
                Message = "Shipment booked successfully via DTDC."
            };
        }






        private async Task<DtdcBookingResponse> CreateShipmentAsync(
    DtdcBookingRequest request,
    CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(request);

            using var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                _settings.BookingUrl);

            //      httpRequest.Headers.Add("api-key", _settings.ApiKey);
            if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                httpRequest.Headers.Add("api-key", _settings.ApiKey);
                httpRequest.Headers.Add("x-access-token", _settings.ApiKey);
            }

            httpRequest.Content = content;

            var response = await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"DTDC booking API failed. Status: {response.StatusCode}, Response: {responseBody}");
            }


            var deserializeOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString 
            };

            var result = JsonSerializer.Deserialize<DtdcBookingResponse>(responseBody, deserializeOptions);


            if (result == null)
            {
                throw new Exception("Invalid response received from DTDC booking API.");
            }

            return result;
        }




        public override async Task<bool> IsServiceableAsync(
            string pickupPincode,
            string deliveryPincode,
            OrderTypeEnum orderType,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pickupPincode) || pickupPincode.Length != 6 ||
                    string.IsNullOrWhiteSpace(deliveryPincode) || deliveryPincode.Length != 6)
                {
                    return false;
                }

                var pincodeRequest = new DtdcPincodeRequest
                {
                    OrgPincode = pickupPincode,
                    DesPincode = deliveryPincode
                };

                var json = JsonSerializer.Serialize(pincodeRequest);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _settings.PincodeUrl);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
                httpRequest.Content = content;

                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("DTDC Pincode API returned error status: {StatusCode}", response.StatusCode);
                    return false;
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<DtdcPincodeResponse>(responseBody);

                if (result != null && result.ZipcodeResp != null && result.ZipcodeResp.Any())
                {
                    var matrix = result.ZipcodeResp.First();
                    bool isRouteOk = matrix.ServFlag?.ToUpper() == "Y";
                    bool isPickupOk = matrix.OrgPin == pickupPincode;
                    bool isDeliveryOk = matrix.DestPin == deliveryPincode;

                    return isRouteOk && isPickupOk && isDeliveryOk;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception caught during DTDC simple serviceability check.");
                return false; 
            }
        }







        public override async Task<CourierLabelResponse> GetLabelAsync(
            CourierLabelRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Streaming real-time verified production label from DTDC infrastructure for AWB: {Awb}", request.AwbNumber);

                if (string.IsNullOrWhiteSpace(request.AwbNumber))
                {
                    return new CourierLabelResponse { Success = false, Message = "Tracking identifier identifier parameter boundary state null." };
                }


                var labelUrl = $"{_settings.BookingUrl?.Replace("/consignment/softdata", "/consignment/shippinglabel/stream")}" +
                               $"?reference_number={request.AwbNumber.Trim()}&label_code=SHIP_LABEL_4X6&label_format=base64";

                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, labelUrl);

                if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
                {
                    httpRequest.Headers.Add("api-key", _settings.ApiKey);
                    httpRequest.Headers.Add("x-access-token", _settings.ApiKey);
                }

                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("DTDC Shipping Label API stream failed. Status: {StatusCode}", response.StatusCode);
                    return new CourierLabelResponse { Success = false, Message = $"DTDC Server failed to stream the requested label. Status: {response.StatusCode}" };
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                using var doc = JsonDocument.Parse(responseBody);
                if (doc.RootElement.TryGetProperty("label", out var labelElement))
                {
                    var base64String = labelElement.GetString() ?? string.Empty;
                    byte[] rawPdfBytes = Convert.FromBase64String(base64String);

                    return new CourierLabelResponse
                    {
                        Success = true,
                        LabelUrl = "",
                        LabelContent = rawPdfBytes, 
                        ContentType = "application/pdf",
                        IsStub = false, 
                        Message = "DTDC dynamic thermal label streamed successfully."
                    };
                }

                return new CourierLabelResponse { Success = false, Message = "Label token missing from JSON response structure." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal crash caught inside DtdcAdapter.GetLabelAsync stream engine.");
                return new CourierLabelResponse { Success = false, Message = $"Internal Exception: {ex.Message}" };
            }
        }








        public override async Task<CourierCancelResponse> CancelAsync(
            CourierCancelRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.AwbNumber))
            {
                return new CourierCancelResponse { Success = false, CourierCode = CourierCode, Message = "AWB number cannot be null." };
            }

            try
            {
                _logger.LogInformation("Dispatching official production cancellation request to DTDC for AWB: {Awb}", request.AwbNumber);

                using (var client = new HttpClient())
                {
                    var cancelPayload = new
                    {
                        AWBNo = new List<string> { request.AwbNumber.Trim() },
                        customerCode = _settings.AccountCode 
                    };

                    var jsonContent = JsonSerializer.Serialize(cancelPayload);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var httpRequest = new HttpRequestMessage(HttpMethod.Post, _settings.CancellationUrl);
                    if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
                    {
                        httpRequest.Headers.Add("api-key", _settings.ApiKey);
                    }

                    httpRequest.Content = content;

                    var response = await client.SendAsync(httpRequest, cancellationToken);
                    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        return new CourierCancelResponse
                        {
                            Success = true,
                            IsStub = false, 
                            CourierCode = CourierCode,
                            Message = "Shipment canceled successfully on DTDC production servers."
                        };
                    }

                    _logger.LogWarning("DTDC Live Cancellation API rejected payload. Status: {Status}, Body: {Body}", response.StatusCode, responseBody);
                    return new CourierCancelResponse { Success = false, CourierCode = CourierCode, Message = $"DTDC Core Rejection: {responseBody}" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal crash caught inside DtdcAdapter.CancelAsync pipeline.");
                return new CourierCancelResponse { Success = false, CourierCode = CourierCode, Message = $"Internal Exception: {ex.Message}" };
            }
        }





        public override async Task<CourierTrackResponse> TrackAsync(
            CourierTrackRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {

                var trackUrl = _settings.TrackingUrl;

                var trackingRequestBody = new
                {
                    trkType = "cnno",
                    strcnno = request.AwbNumber.Trim(),
                    addtnlDtl = "Y"
                };

                var json = JsonSerializer.Serialize(trackingRequestBody);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, trackUrl);
                httpRequest.Headers.Add("x-access-token", _settings.TrackingToken);
                httpRequest.Content = content;

                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("DTDC Track API failed. Status: {StatusCode}", response.StatusCode);
                    return new CourierTrackResponse { Success = false, Message = "Courier server connectivity error." };
                }

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                if (root.TryGetProperty("statusFlag", out var flagElement) && !flagElement.GetBoolean())
                {
                    return new CourierTrackResponse { Success = false, Message = "No real data found for this AWB number on DTDC network." };
                }

                // 2. Extracting root summary details from Page 7 trackHeader node
                var headerNode = root.GetProperty("trackHeader");
                string dtdcRootStatus = headerNode.GetProperty("strStatus").GetString() ?? "Booked";

                var dynamicEventList = new List<CourierTrackEvent>();

                if (root.TryGetProperty("trackDetails", out var detailsElement) && detailsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var scan in detailsElement.EnumerateArray())
                    {
                        string actionCode = scan.GetProperty("strCode").GetString() ?? "";
                        string actionName = scan.GetProperty("strAction").GetString() ?? "";
                        string actionDateStr = scan.GetProperty("strActionDate").GetString() ?? "";
                        string actionTimeStr = scan.GetProperty("strActionTime").GetString() ?? ""; 
                        string remarks = scan.GetProperty("sTrRemarks").GetString() ?? "";

                        
                        DateTime eventDateTime = DateTime.UtcNow;
                        if (actionDateStr.Length == 8)
                        {
                            int day = int.Parse(actionDateStr.Substring(0, 2));
                            int month = int.Parse(actionDateStr.Substring(2, 2));
                            int year = int.Parse(actionDateStr.Substring(4, 4));
                            eventDateTime = new DateTime(year, month, day);
                        }
                        string sysStatusCode = "IN-TRANSIT";
                        if (actionCode == "BKD") sysStatusCode = "BOOKED";
                        else if (actionCode == "DLV") sysStatusCode = "DELIVERED";
                        else if (actionCode == "NONDLV") sysStatusCode = "NDR";
                        else if (actionCode == "RTO") sysStatusCode = "RTO";

                        dynamicEventList.Add(new CourierTrackEvent
                        {
                            EventTime = eventDateTime,
                            Status = actionName,
                            StatusCode = sysStatusCode,
                            Location = scan.GetProperty("strOrigin").GetString() ?? "DTDC Facility Hub",
                            Description = !string.IsNullOrWhiteSpace(remarks) ? remarks : $"Shipment update: {actionName}"
                        });
                    }
                }

                return new CourierTrackResponse
                {
                    Success = true,
                    IsStub = false,
                    CurrentStatus = dtdcRootStatus.ToUpper(),
                    Events = dynamicEventList,
                    Message = "Live tracking events synchronized successfully from DTDC servers."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Crash caught inside DtdcAdapter.TrackAsync processing.");
                return new CourierTrackResponse { Success = false, IsStub = true, Message = ex.Message };
            }
        }




        public override async Task<bool> RequestNdrReAttemptAsync(
            string awbNumber,
            string actionType,
            string remarks,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Dispatched DTDC NDR Instruction Payload for AWB: {Awb}", awbNumber);

                var ndrPayloadItem = new
                {
                    consgNumber = awbNumber.Trim(),
                    custCode = _settings.AccountCode,
                    rtoAction = "1", 
                    remarks = !string.IsNullOrWhiteSpace(remarks) ? remarks.Trim() : "Seller requested reattempt cycle."
                };

                var payloadCollection = new[] { ndrPayloadItem };
                var json = JsonSerializer.Serialize(payloadCollection);

                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _settings.NdrUpdateUrl);

                var authBytes = Encoding.ASCII.GetBytes($"{_settings.NdrUsername}:{_settings.NdrPassword}");
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
                httpRequest.Content = content;

                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("DTDC NDR API HTTP Fault. Status Code: {StatusCode}", response.StatusCode);
                    return false;
                }

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;
                if (root.TryGetProperty("status", out var statusElement) &&
                    string.Equals(statusElement.GetString(), "OK", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("DTDC live NDR instruction accepted successfully by remote routers.");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error caught inside DtdcAdapter.RequestNdrReAttemptAsync processing thread.");
                return false;
            }
        }

        public override async Task<CourierRateResponse> GetRatesAsync(
            CourierRateRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Dispatched DTDC live dynamic Rate Calculator payload for Account: {Code}", _settings.AccountCode);

                var rateUrl = _settings.RateCalculatorUrl;
                if (string.IsNullOrWhiteSpace(rateUrl))
                {
                    throw new InvalidOperationException("DTDC Rate Calculator API url context reference missing in configuration matrix settings.");
                }
                var rateRequest = new
                {
                    originPincode = request.OriginPincode.Trim(),
                    destPincode = request.DestinationPincode.Trim(),
                    weight = Convert.ToDecimal(request.WeightKg > 0 ? request.WeightKg : 0.5m),
                    expectedBookingDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    invoiceValue = Convert.ToDecimal(request.CodAmount > 0 ? request.CodAmount : 100.0m),
                    mode = "SURFACE",
                    pieces = (request.TotalQuantity > 0 ? request.TotalQuantity : 1).ToString(),
                    documentType = "N",
                    insured = "N",
                    insuredBy = "",
                    codAmount = request.IsCod ? (request.CodAmount ?? 0m).ToString("0") : "0",
                    customerCode = !string.IsNullOrWhiteSpace(_settings.AccountCode) ? _settings.AccountCode : "GL19990"
                };

                var json = JsonSerializer.Serialize(rateRequest);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, rateUrl);
                httpRequest.Headers.Add("x-access-token", _settings.RateToken);
                httpRequest.Content = content;


                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("DTDC Rate API returned explicit server network fault string: {StatusCode}", response.StatusCode);

                    return new CourierRateResponse
                    {
                        Success = false,
                        Message = $"DTDC remote server gateway execution exception path. HTTP Status: {response.StatusCode}"
                    };
                }
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                bool isApiSuccess = root.TryGetProperty("status", out var statusElement) && statusElement.GetBoolean();
                if (!isApiSuccess)
                {
                    string runtimeErrorMessage = root.TryGetProperty("errorMessage", out var errMsgElement)
                        ? errMsgElement.GetString() ?? "Validation failure"
                        : "DTDC core gateway declined query constraints.";

                    _logger.LogWarning("DTDC Rate Calculator calculation declined: {Message}", runtimeErrorMessage);
                    return new CourierRateResponse { Success = false, Message = runtimeErrorMessage };
                }

                var parsedServiceRatesList = new List<TryNextPost.Application.DTO.Courier.CourierRateOption>();

                if (root.TryGetProperty("serviceCode", out var serviceListElement) && serviceListElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var serviceItem in serviceListElement.EnumerateArray())
                    {
                        decimal totalAmountPayable = 0m;
                        if (serviceItem.TryGetProperty("totalAmount", out var amtElement))
                        {
                            if (amtElement.ValueKind == JsonValueKind.Number)
                                totalAmountPayable = amtElement.GetDecimal();
                            else if (amtElement.ValueKind == JsonValueKind.String && decimal.TryParse(amtElement.GetString(), out var parsedAmt))
                                totalAmountPayable = parsedAmt;
                        }

                        if (totalAmountPayable <= 0.0m) continue;

                        string targetServiceCode = serviceItem.TryGetProperty("serviceCode", out var codeElement) ? codeElement.GetString() ?? "" : "";
                        string readableServiceName = serviceItem.TryGetProperty("serviceName", out var nameElement) ? nameElement.GetString() ?? "DTDC Cargo Service" : "DTDC Cargo Service";
                        int estimatedTransitDays = serviceItem.TryGetProperty("tat", out var tatElement) ? tatElement.GetInt32() : 3;

                        parsedServiceRatesList.Add(new TryNextPost.Application.DTO.Courier.CourierRateOption
                        {
                            ServiceName = readableServiceName,
                            ServiceCode = targetServiceCode,
                            TotalCharge = totalAmountPayable,
                            CodCharge = request.IsCod ? (request.CodChargeValue > 0 ? request.CodChargeValue : 30.0m) : 0.0m,
                            EstimatedDays = estimatedTransitDays,
                            IsStub = false,
                            RateId = targetServiceCode,
                            RequestToken = _settings.AccountCode ?? "DTDC_TOKEN"
                        });
                    }
                }
                return new CourierRateResponse
                {
                    Success = parsedServiceRatesList.Any(),
                    CourierCode = CourierCode,
                    Rates = parsedServiceRatesList,
                    Message = "DTDC live dynamic rates extracted successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal crash inside DtdcAdapter.GetRatesAsync processing pipeline threads boundary.");
                return new CourierRateResponse { Success = false, Message = $"Internal Exception Gateway Fault: {ex.Message}" };
            }
        }

    }
}
