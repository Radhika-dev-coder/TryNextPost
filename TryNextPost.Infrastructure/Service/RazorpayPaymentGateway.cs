using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Application.DTO.Payment;
using TryNextPost.Application.IServices.Interface.IPayment;
using TryNextPost.Domain.Common;

namespace TryNextPost.Infrastructure.Service
{
    public class RazorpayPaymentGateway : IRazorpayPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly RazorpaySettings _settings;

        public RazorpayPaymentGateway(HttpClient httpClient, IOptions<RazorpaySettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;

            var baseUrl = string.IsNullOrWhiteSpace(_settings.BaseUrl)
                ? "https://api.razorpay.com/v1"
                : _settings.BaseUrl.TrimEnd('/');

            _httpClient.BaseAddress = new Uri(baseUrl.EndsWith("/v1", StringComparison.OrdinalIgnoreCase)
                ? baseUrl + "/"
                : baseUrl + "/v1/");
        }

        public async Task<RazorpayCreateOrderResult> CreateOrderAsync(
            int amountPaise,
            string receipt,
            IDictionary<string, string>? notes = null,
            CancellationToken cancellationToken = default)
        {
            EnsureApiCredentials();

            var payload = new Dictionary<string, object?>
            {
                ["amount"] = amountPaise,
                ["currency"] = "INR",
                ["receipt"] = receipt,
                ["notes"] = notes ?? new Dictionary<string, string>()
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "orders");
            request.Headers.Authorization = CreateBasicAuthHeader();
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Razorpay CreateOrder failed ({(int)response.StatusCode}): {Truncate(body, 500)}");
            }

            var order = JsonSerializer.Deserialize<RazorpayOrderApiResponse>(body, JsonOptions)
                ?? throw new InvalidOperationException("Razorpay CreateOrder returned an empty response.");

            if (string.IsNullOrWhiteSpace(order.Id))
                throw new InvalidOperationException("Razorpay CreateOrder response missing order id.");

            return new RazorpayCreateOrderResult
            {
                OrderId = order.Id,
                AmountInPaise = order.Amount,
                Currency = string.IsNullOrWhiteSpace(order.Currency) ? "INR" : order.Currency,
                Receipt = order.Receipt ?? receipt,
                Status = order.Status ?? string.Empty
            };
        }

        public bool VerifyWebhookSignature(string rawBody, string signature)
        {
            if (string.IsNullOrWhiteSpace(_settings.WebhookSecret))
                throw new InvalidOperationException(
                    "Razorpay WebhookSecret is not configured. Set Razorpay:WebhookSecret (User Secrets or environment).");

            if (string.IsNullOrWhiteSpace(rawBody) || string.IsNullOrWhiteSpace(signature))
                return false;

            var expected = ComputeHmacSha256Hex(rawBody, _settings.WebhookSecret);
            return FixedTimeEqualsHex(expected, signature.Trim());
        }

        public bool VerifyPaymentSignature(string orderId, string paymentId, string signature)
        {
            EnsureApiCredentials();

            if (string.IsNullOrWhiteSpace(orderId)
                || string.IsNullOrWhiteSpace(paymentId)
                || string.IsNullOrWhiteSpace(signature))
            {
                return false;
            }

            var payload = $"{orderId}|{paymentId}";
            var expected = ComputeHmacSha256Hex(payload, _settings.KeySecret);
            return FixedTimeEqualsHex(expected, signature.Trim());
        }

        private void EnsureApiCredentials()
        {
            if (string.IsNullOrWhiteSpace(_settings.KeyId) || string.IsNullOrWhiteSpace(_settings.KeySecret))
                throw new InvalidOperationException(SystemMessage.RazorpayCredentialsMissing);
        }

        private AuthenticationHeaderValue CreateBasicAuthHeader()
        {
            var token = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_settings.KeyId}:{_settings.KeySecret}"));
            return new AuthenticationHeaderValue("Basic", token);
        }

        private static string ComputeHmacSha256Hex(string data, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var hash = HMACSHA256.HashData(keyBytes, dataBytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private static bool FixedTimeEqualsHex(string expectedHex, string actual)
        {
            // Razorpay sends lowercase hex; normalize both sides.
            var actualNorm = actual.Trim().ToLowerInvariant();
            if (expectedHex.Length != actualNorm.Length)
                return false;

            var a = Encoding.UTF8.GetBytes(expectedHex);
            var b = Encoding.UTF8.GetBytes(actualNorm);
            return CryptographicOperations.FixedTimeEquals(a, b);
        }

        private static string Truncate(string value, int max)
            => value.Length <= max ? value : value[..max] + "...";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private sealed class RazorpayOrderApiResponse
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("amount")]
            public int Amount { get; set; }

            [JsonPropertyName("currency")]
            public string? Currency { get; set; }

            [JsonPropertyName("receipt")]
            public string? Receipt { get; set; }

            [JsonPropertyName("status")]
            public string? Status { get; set; }
        }
    }
}
