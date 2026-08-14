using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Infrastructure.CourierAdapters
{
    public sealed class DtdcAdapter : CourierAdapterBase
    {
        private readonly DtdcSettings _settings;

        public DtdcAdapter(
            IOptions<CourierSettings> options,
            ILogger<DtdcAdapter> logger,
            IOrderRepository orderRepository)
            : base(logger, orderRepository)
        {
            _settings = options.Value.Dtdc;
        }

        public override string CourierCode =>CourierCodes.Dtdc;

        protected override bool IsConfigured =>
     _settings.Enabled &&
     !string.IsNullOrWhiteSpace(_settings.BaseUrl) &&
     !string.IsNullOrWhiteSpace(_settings.ApiKey) &&
     !string.IsNullOrWhiteSpace(_settings.ApiSecret);
    }
}
