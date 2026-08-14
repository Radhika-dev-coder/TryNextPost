using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Infrastructure.CourierAdapters
{
    public sealed class EkartAdapter : CourierAdapterBase
    {
        private readonly CourierProviderSettings _settings;

        public EkartAdapter(IOptions<CourierSettings> options, ILogger<EkartAdapter> logger, IOrderRepository orderRepository)
            : base(logger, orderRepository)
        {
            _settings = options.Value.Ekart;
        }

        public override string CourierCode => CourierCodes.Ekart;

        protected override CourierProviderSettings Settings => _settings;
    }
}
