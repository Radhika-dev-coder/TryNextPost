using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Infrastructure.CourierAdapters
{
    public sealed class IndiaPostAdapter : CourierAdapterBase
    {
        private readonly IndiaPostSettings _settings;

        public IndiaPostAdapter(
            IOptions<CourierSettings> options,
            ILogger<IndiaPostAdapter> logger,
            IOrderRepository orderRepository)
            : base(logger, orderRepository)
        {
            _settings = options.Value.IndiaPost;
        }

        public override string CourierCode =>CourierCodes.IndiaPost;

        protected override bool IsConfigured =>
    _settings.Enabled &&
    !string.IsNullOrWhiteSpace(_settings.BaseUrl) &&
    !string.IsNullOrWhiteSpace(_settings.ApiKey) &&
    !string.IsNullOrWhiteSpace(_settings.ApiSecret);
    }
}
