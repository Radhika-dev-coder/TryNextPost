using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Domain.Common;

namespace TryNextPost.Infrastructure.CourierAdapters
{
    public sealed class XpressbeesAdapter : CourierAdapterBase
    {
        private readonly CourierProviderSettings _settings;

        public XpressbeesAdapter(IOptions<CourierSettings> options, ILogger<XpressbeesAdapter> logger)
            : base(logger)
        {
            _settings = options.Value.Xpressbees;
        }

        public override string CourierCode => CourierCodes.Xpressbees;

        protected override CourierProviderSettings Settings => _settings;
    }
}
