using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Application.DTO.Courier;
using TryNextPost.Domain.Common;

namespace TryNextPost.Infrastructure.CourierAdapters
{
    /// <summary>
    /// Delhivery courier adapter.
    /// Empty BaseUrl/ApiKey → stub rates/booking/label/cancel/track via <see cref="CourierAdapterBase"/>.
    /// When credentials are set, Internal methods use HttpClient scaffolding and throw a clear
    /// NotImplementedException until the real Delhivery endpoints are mapped.
    /// </summary>
    public sealed class DelhiveryAdapter : CourierAdapterBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CourierProviderSettings _settings;

        public DelhiveryAdapter(
            IHttpClientFactory httpClientFactory,
            IOptions<CourierSettings> options,
            ILogger<DelhiveryAdapter> logger)
            : base(logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value.Delhivery;
        }

        public override string CourierCode => CourierCodes.Delhivery;

        protected override CourierProviderSettings Settings => _settings;

        /// <summary>
        /// Phase 2 plug-in: Delhivery rate API.
        /// Without real API mapping this throws NotImplemented (caught by rates loop → skip).
        /// Leave BaseUrl empty in appsettings to keep using stubs.
        /// </summary>
        protected override async Task<CourierRateResponse> GetRatesInternalAsync(
            CourierRateRequest request,
            CancellationToken cancellationToken)
        {
            // Credentials present (otherwise base would have returned stub).
            var client = CreateClient();

            // TODO: Map to Delhivery pincode serviceability / rate API, e.g.:
            //   GET {BaseUrl}/c/api/pin-codes/json/?filter_codes={destination}
            //   or rate calculator endpoint with weight/COD.
            //   var response = await client.GetAsync($"...?", cancellationToken);
            //   Parse JSON → CourierRateResponse { IsStub = false, Rates = [...] }
            _ = client;
            _ = request;
            _ = cancellationToken;

            EnsureApiReady(nameof(GetRatesAsync));
            return await Task.FromException<CourierRateResponse>(new NotImplementedException());
        }

        /// <summary>
        /// Phase 2 plug-in: Delhivery create shipment / waybill API.
        /// </summary>
        protected override async Task<CourierBookShipmentResponse> BookShipmentInternalAsync(
            CourierBookShipmentRequest request,
            CancellationToken cancellationToken)
        {
            var client = CreateClient();

            // TODO: POST {BaseUrl}/api/cmu/create.json (or current Delhivery create shipment URL)
            //   Headers: Authorization Token {ApiKey}
            //   Body: pickup + delivery from request, weight, payment mode.
            //   Map waybill / AWB → CourierBookShipmentResponse { IsStub = false }.
            _ = client;
            _ = request;
            _ = cancellationToken;

            EnsureApiReady(nameof(BookShipmentAsync));
            return await Task.FromException<CourierBookShipmentResponse>(new NotImplementedException());
        }

        protected override async Task<CourierLabelResponse> GetLabelInternalAsync(
            CourierLabelRequest request,
            CancellationToken cancellationToken)
        {
            var client = CreateClient();

            // TODO: GET packing slip / label PDF for AWB from Delhivery packing slip API.
            _ = client;
            _ = request;
            _ = cancellationToken;

            EnsureApiReady(nameof(GetLabelAsync));
            return await Task.FromException<CourierLabelResponse>(new NotImplementedException());
        }

        protected override async Task<CourierCancelResponse> CancelInternalAsync(
            CourierCancelRequest request,
            CancellationToken cancellationToken)
        {
            var client = CreateClient();

            // TODO: POST Delhivery cancel / edit package API for AWB.
            _ = client;
            _ = request;
            _ = cancellationToken;

            EnsureApiReady(nameof(CancelAsync));
            return await Task.FromException<CourierCancelResponse>(new NotImplementedException());
        }

        protected override async Task<CourierTrackResponse> TrackInternalAsync(
            CourierTrackRequest request,
            CancellationToken cancellationToken)
        {
            var client = CreateClient();

            // TODO: GET {BaseUrl}/api/v1/packages/json/?waybill={awb}
            //   Map scans → CourierTrackResponse events, IsStub = false.
            _ = client;
            _ = request;
            _ = cancellationToken;

            EnsureApiReady(nameof(TrackAsync));
            return await Task.FromException<CourierTrackResponse>(new NotImplementedException());
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient(nameof(DelhiveryAdapter));

            if (!string.IsNullOrWhiteSpace(Settings.BaseUrl))
                client.BaseAddress = new Uri(Settings.BaseUrl.TrimEnd('/') + "/");

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrWhiteSpace(Settings.ApiKey))
            {
                // Delhivery typically uses: Authorization: Token <apiKey>
                client.DefaultRequestHeaders.Remove("Authorization");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Token {Settings.ApiKey}");
            }

            return client;
        }
    }
}
