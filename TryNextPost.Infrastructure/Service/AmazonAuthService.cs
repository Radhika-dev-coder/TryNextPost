using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TryNextPost.Application.Common;
using TryNextPost.Application.IServices.Interface;

namespace TryNextPost.Infrastructure.Service
{
    public class AmazonAuthService : IAmazonAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AmazonShippingSettings _settings;

        public AmazonAuthService(
            HttpClient httpClient,
            IOptions<AmazonShippingSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<string> GetAccessTokenAsync(   CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_settings.LwaBaseUrl}/auth/o2/token");

            request.Content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = _settings.RefreshToken,
                    ["client_id"] = _settings.ClientId,
                    ["client_secret"] = _settings.ClientSecret
                });

            var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Amazon LWA failed. " +
                    $"Status: {(int)response.StatusCode}. " +
                    $"Response: {responseBody}");
            }

            var tokenResponse =
                JsonSerializer.Deserialize<AmazonTokenResponse>(
                    responseBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (string.IsNullOrWhiteSpace(
                tokenResponse?.AccessToken))
            {
                throw new InvalidOperationException(
                    "Amazon LWA response did not contain an access token.");
            }

            return tokenResponse.AccessToken;
        }
    }
}
public class AmazonTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

