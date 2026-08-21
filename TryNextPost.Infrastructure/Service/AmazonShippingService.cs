using Azure.Core;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TryNextPost.Application.Common;
using TryNextPost.Application.DTO.AmazonDto;
using TryNextPost.Application.IServices.Interface;

namespace TryNextPost.Infrastructure.Service
{
    public class AmazonShippingService : IAmazonShippingService
    {
        private readonly HttpClient _httpClient;
        private readonly IAmazonAuthService _authService;
        private readonly AmazonShippingSettings _settings;

        public AmazonShippingService(
            HttpClient httpClient,
            IAmazonAuthService authService,
            IOptions<AmazonShippingSettings> options)
        {
            _httpClient = httpClient;
            _authService = authService;
            _settings = options.Value;
        }

        public async Task<AmazonGetRatesResponse> GetRatesAsync(AmazonGetRatesRequest request,  CancellationToken cancellationToken = default)
        {
            var accessToken =
                await _authService.GetAccessTokenAsync(
                    cancellationToken);

            var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_settings.ShippingBaseUrl}/shipping/v2/shipments/rates");

            httpRequest.Headers.Add(
                "x-amz-access-token",
                accessToken);

            httpRequest.Headers.Add(
                "x-amzn-shipping-business-id",
                _settings.ShippingBusinessId);

            httpRequest.Content =
                JsonContent.Create(request);

            var response = await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException( $"Amazon GetRates failed. " + $"Status: {(int)response.StatusCode}. " + $"Response: {responseBody}");
            }

            var result =
                JsonSerializer.Deserialize<AmazonGetRatesResponse>(
                    responseBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (result == null)
            {
                throw new InvalidOperationException(
                    "Amazon GetRates returned an empty response.");
            }

            return result;
        }


        public async Task<AmazonBookShipmentResponse> BookShipmentAsync(  AmazonBookShipmentRequest request, CancellationToken cancellationToken = default)
        {
            var accessToken =
                await _authService.GetAccessTokenAsync(cancellationToken);

            var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_settings.ShippingBaseUrl}/shipping/v2/shipments");

            httpRequest.Headers.Add(
                "x-amz-access-token",
                accessToken);

            httpRequest.Headers.Add(
                "x-amzn-shipping-business-id",
                _settings.ShippingBusinessId);

            httpRequest.Content = JsonContent.Create(request);

            var response = await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Amazon BookShipment failed. " +
                    $"Status: {(int)response.StatusCode}. " +
                    $"Response: {responseBody}");
            }

            var result =
                JsonSerializer.Deserialize<AmazonBookShipmentResponse>(
                    responseBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (result == null)
            {
                throw new InvalidOperationException(
                    "Amazon BookShipment returned an empty response.");
            }

            return result;
        }

        public async Task<AmazonPurchaseShipmentResponse> PurchaseShipmentAsync(
          AmazonPurchaseShipmentRequest request,
          CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var accessToken = await _authService.GetAccessTokenAsync(
                cancellationToken);

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_settings.ShippingBaseUrl}/shipping/v2/shipments");

            httpRequest.Headers.Add(
                "x-amz-access-token",
                accessToken);

            httpRequest.Headers.Add(
                "x-amzn-shipping-business-id",
                "AmazonShipping_IN");

            httpRequest.Content = JsonContent.Create(request);

            using var response = await _httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(
                cancellationToken);

            if (string.IsNullOrWhiteSpace(responseBody))
            {
                throw new InvalidOperationException(
                    "Amazon purchase shipment returned an empty response.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Amazon purchase shipment failed. " +
                    $"StatusCode: {(int)response.StatusCode} ({response.StatusCode}). " +
                    $"Response: {responseBody}");
            }

            AmazonPurchaseShipmentApiResponse? apiResult;

            try
            {
                apiResult =
                    JsonSerializer.Deserialize<AmazonPurchaseShipmentApiResponse>(
                        responseBody,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    "Invalid JSON received from Amazon purchase shipment API.",
                    ex);
            }

            if (apiResult?.Payload is null)
            {
                throw new InvalidOperationException(
                    "Amazon purchase shipment response did not contain a payload.");
            }

            // Decode Base64 encoded package document contents.
            if (apiResult.Payload.PackageDocumentDetails is not null)
            {
                foreach (var packageDetail in apiResult.Payload.PackageDocumentDetails)
                {
                    if (packageDetail.PackageDocuments is null)
                    {
                        continue;
                    }

                    foreach (var document in packageDetail.PackageDocuments)
                    {
                        if (string.IsNullOrWhiteSpace(document.Contents))
                        {
                            continue;
                        }

                        try
                        {
                            document.ContentBytes = Convert.FromBase64String(document.Contents);
                        }
                        catch (FormatException ex)
                        {
                            throw new InvalidOperationException(
                                $"Invalid Base64 content received for " +
                                $"package document type '{document.Type}'.",
                                ex);
                        }
                    }
                }
            }

            return apiResult.Payload;
        }
        private static string DecodeBase64IfRequired(string value)
        {
            var trimmedValue = value.Trim();

            try
            {
                var decodedBytes = Convert.FromBase64String(trimmedValue);
                var decodedValue = Encoding.UTF8.GetString(decodedBytes).Trim();

                if (decodedValue.StartsWith("{") ||
                    decodedValue.StartsWith("["))
                {
                    return decodedValue;
                }
            }
            catch (FormatException)
            {
                // Response is already plain JSON.
            }

            return trimmedValue;
        }
        public async Task<AmazonCreateShipmentResponse> CreateShipmentAsync(  AmazonCreateShipmentRequest request,   CancellationToken cancellationToken = default)
        {
            var accessToken =
                await _authService.GetAccessTokenAsync(cancellationToken);

            var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_settings.ShippingBaseUrl}/shipping/v2/shipments");

            httpRequest.Headers.Add(
                "x-amz-access-token",
                accessToken);

            httpRequest.Headers.Add(
                "x-amzn-shipping-business-id",
                _settings.ShippingBusinessId);
            var debugJson = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(debugJson);
            httpRequest.Content = JsonContent.Create(request);

            var response = await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Amazon CreateShipment failed. " +
                    $"Status: {(int)response.StatusCode}. " +
                    $"Response: {responseBody}");
            }

            var result =
                JsonSerializer.Deserialize<AmazonCreateShipmentResponse>(
                    responseBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (result == null)
            {
                throw new InvalidOperationException(
                    "Amazon CreateShipment returned an empty response.");
            }

            return result;
        }

        public async Task<AmazonGetLabelResponse> GetLabelAsync(
    AmazonGetLabelRequest request,
    CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.ShipmentId))
                throw new ArgumentException(
                    "Amazon ShipmentId is required.",
                    nameof(request));

            var accessToken =
                await _authService.GetAccessTokenAsync(cancellationToken);

            var url =
                $"{_settings.ShippingBaseUrl}/shipping/v2/shipments/{Uri.EscapeDataString(request.ShipmentId)}/label";

            var httpRequest = new HttpRequestMessage(
                HttpMethod.Get,
                url);

            httpRequest.Headers.Add(
                "x-amz-access-token",
                accessToken);

            httpRequest.Headers.Add(
                "x-amzn-shipping-business-id",
                _settings.ShippingBusinessId);

            var response = await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Amazon GetLabel failed. " +
                    $"Status: {(int)response.StatusCode}. " +
                    $"Response: {responseBody}");
            }

            var result =
                JsonSerializer.Deserialize<AmazonGetLabelResponse>(
                    responseBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (result == null)
            {
                throw new InvalidOperationException(
                    "Amazon GetLabel returned an empty response.");
            }

            return result;
        }

        public async Task<AmazonCancelShipmentResponse> CancelShipmentAsync( AmazonCancelShipmentRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.ShipmentId))
                throw new ArgumentException(
                    "Amazon ShipmentId is required.",
                    nameof(request));

            var accessToken =
                await _authService.GetAccessTokenAsync(cancellationToken);

            var url =
                $"{_settings.ShippingBaseUrl}/shipping/v2/shipments/" +
                $"{Uri.EscapeDataString(request.ShipmentId)}";

            var httpRequest = new HttpRequestMessage(
                HttpMethod.Delete,
                url);

            httpRequest.Headers.Add(
                "x-amz-access-token",
                accessToken);

            httpRequest.Headers.Add(
                "x-amzn-shipping-business-id",
                _settings.ShippingBusinessId);

            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                httpRequest.Content = JsonContent.Create(new
                {
                    reason = request.Reason
                });
            }

            var response = await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Amazon CancelShipment failed. " +
                    $"Status: {(int)response.StatusCode}. " +
                    $"Response: {responseBody}");
            }

            var result =
                JsonSerializer.Deserialize<AmazonCancelShipmentResponse>(
                    responseBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (result == null)
            {
                throw new InvalidOperationException(
                    "Amazon CancelShipment returned an empty response.");
            }

            return result;
        }

        public async Task<AmazonTrackShipmentResponse> TrackShipmentAsync(
    AmazonTrackShipmentRequest request,
    CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.ShipmentId))
                throw new ArgumentException(
                    "Amazon ShipmentId is required.",
                    nameof(request));

            var accessToken =
                await _authService.GetAccessTokenAsync(cancellationToken);

            var url =
                $"{_settings.ShippingBaseUrl}/shipping/v2/shipments/" +
                $"{Uri.EscapeDataString(request.ShipmentId)}";

            var httpRequest = new HttpRequestMessage(
                HttpMethod.Get,
                url);

            httpRequest.Headers.Add(
                "x-amz-access-token",
                accessToken);

            httpRequest.Headers.Add(
                "x-amzn-shipping-business-id",
                _settings.ShippingBusinessId);

            var response = await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Amazon TrackShipment failed. " +
                    $"Status: {(int)response.StatusCode}. " +
                    $"Response: {responseBody}");
            }

            var result =
                JsonSerializer.Deserialize<AmazonTrackShipmentResponse>(
                    responseBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (result == null)
            {
                throw new InvalidOperationException(
                    "Amazon TrackShipment returned an empty response.");
            }

            return result;
        }
    }
}
