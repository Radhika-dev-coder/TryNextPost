using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Application.DTO.Courier;
using TryNextPost.Domain.Common;

namespace TryNextPost.Infrastructure.CourierAdapters
{
    public sealed class XpressbeesAdapter : CourierAdapterBase
    {
        private readonly CourierProviderSettings _settings;
        private readonly HttpClient _httpClient;

        public XpressbeesAdapter(
            IOptions<CourierSettings> options,
            ILogger<XpressbeesAdapter> logger)
            : base(logger)
        {
            _settings = options.Value.Xpressbees;
            _httpClient = new HttpClient();
        }

        public override string CourierCode => CourierCodes.Xpressbees;

        protected override CourierProviderSettings Settings => _settings;

        protected override async Task<CourierBookShipmentResponse> BookShipmentInternalAsync(
            CourierBookShipmentRequest request,
            CancellationToken cancellationToken)
        {
            var token = await GenerateTokenAsync();

            var fullAddress = string.Join(", ", new[]
            {
                request.DeliveryAddressLine1,
                request.DeliveryAddressLine2,
                request.DeliveryCity,
                request.DeliveryState
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

            var body = new
            {
                order_number = request.OrderRef,
                name = request.DeliveryName,
                address = fullAddress,
                mobile = request.DeliveryPhone,
                pincode = request.DeliveryPincode,
                payment_mode = request.IsCod ? "COD" : "Prepaid",

                weight = request.WeightKg,
                cod_amount = request.IsCod ? request.CodAmount : 0,
                invoice_value = request.InvoiceValue,
                product_description = request.ProductDescription
            };

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync(
                _settings.ForwardUrl,
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
                cancellationToken
            );

            var json = await response.Content.ReadAsStringAsync();
            string? awb = null;
            string? message = null;

            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("data", out var data))
            {
                if (data.TryGetProperty("awb_number", out var awbProp))
                {
                    awb = awbProp.GetString();
                }
            }

            if (doc.RootElement.TryGetProperty("message", out var msgProp))
            {
                message = msgProp.GetString();
            }

            return new CourierBookShipmentResponse
            {
                Success = response.IsSuccessStatusCode && !string.IsNullOrEmpty(awb),
                CourierCode = CourierCode,
                AwbNumber = awb,
                CourierReference = request.OrderRef,
                Message = message,
                RawResponse = json
            };
        }

        private async Task<string> GenerateTokenAsync()
        {
            var body = new
            {
                email = _settings.ApiKey,
                password = _settings.ApiSecret
            };

            var response = await _httpClient.PostAsync(
                _settings.TokenUrl,
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            );

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("token").GetString() ?? "";
        }
    }
}