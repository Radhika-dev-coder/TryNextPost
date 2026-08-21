using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Application.DTO.Courier;
using TryNextPost.Application.DTO.Courier.XpressBees;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Infrastructure.CourierAdapters
{
    public sealed class DelhiveryAdapter : CourierAdapterBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DelhiverySettings _settings;
       // private readonly IOrderRepository _orderRepository;
        public DelhiveryAdapter(
            IHttpClientFactory httpClientFactory,
            IOptions<CourierSettings> options,
            ILogger<DelhiveryAdapter> logger,
            IOrderRepository orderRepository)
            : base(logger, orderRepository)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value.Delhivery;
        }

        public override string CourierCode =>CourierCodes.BlueDart;
        protected override bool IsConfigured =>
    _settings.Enabled &&
    !string.IsNullOrWhiteSpace(_settings.BaseUrl) &&
    !string.IsNullOrWhiteSpace(_settings.ApiKey) &&
    !string.IsNullOrWhiteSpace(_settings.ApiSecret);


        protected override async Task<CourierRateResponse> GetRatesInternalAsync(
            CourierRateRequest request,
            CancellationToken cancellationToken)
        {
            var client = CreateClient();
            _ = client;
            _ = request;
            _ = cancellationToken;

            EnsureApiReady(nameof(GetRatesAsync));
            return await Task.FromException<CourierRateResponse>(new NotImplementedException());
        }


        protected override async Task<CourierBookShipmentResponse> BookShipmentInternalAsync(
            CourierShipmentRequest request,
            CancellationToken cancellationToken)
        {
            var client = CreateClient();

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
            _ = client;
            _ = request;
            _ = cancellationToken;
            EnsureApiReady(nameof(TrackAsync));
            return await Task.FromException<CourierTrackResponse>(new NotImplementedException());
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient(nameof(DelhiveryAdapter));

            if (!string.IsNullOrWhiteSpace(_settings.BaseUrl))
                client.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/");

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            {            
                client.DefaultRequestHeaders.Remove("Authorization");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Token {_settings.ApiKey}");
            }
            return client;
        }

        public override async Task<bool> RequestNdrReAttemptAsync(string awbNumber, string actionType, string remarks, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Delhivery NDR workflow not integrated yet.");
        }

    }
}
