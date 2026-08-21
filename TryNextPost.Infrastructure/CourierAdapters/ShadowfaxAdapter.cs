using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.Repository;

namespace TryNextPost.Infrastructure.CourierAdapters
{
    public sealed class ShadowfaxAdapter : CourierAdapterBase
    {
        private readonly ShadowfaxSettings _settings;

        public ShadowfaxAdapter(
            IOptions<CourierSettings> options,
            ILogger<ShadowfaxAdapter> logger,
            IOrderRepository orderRepository)
            : base(logger, orderRepository)
        {
            _settings = options.Value.Shadowfax;
        }

        public override string CourierCode =>CourierCodes.Shadowfax;

        protected override bool IsConfigured =>
    _settings.Enabled &&
    !string.IsNullOrWhiteSpace(_settings.BaseUrl) &&
    !string.IsNullOrWhiteSpace(_settings.ApiKey) &&
    !string.IsNullOrWhiteSpace(_settings.ApiSecret);

        public override async Task<bool> RequestNdrReAttemptAsync(string awbNumber, string actionType, string remarks, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Shadowfax NDR workflow not integrated yet.");
        }

    }
}
