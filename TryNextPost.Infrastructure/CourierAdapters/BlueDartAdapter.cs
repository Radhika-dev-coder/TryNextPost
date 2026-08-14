using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Infrastructure.CourierAdapters
{
    public sealed class BlueDartAdapter : CourierAdapterBase
    {
        private readonly CourierProviderSettings _settings;

        public BlueDartAdapter(IOptions<CourierSettings> options, ILogger<BlueDartAdapter> logger, IOrderRepository orderRepository)
            : base(logger, orderRepository)
        {
            _settings = options.Value.BlueDart;
        }

        public override string CourierCode => CourierCodes.BlueDart;

        protected override CourierProviderSettings Settings => _settings;
    }
}
