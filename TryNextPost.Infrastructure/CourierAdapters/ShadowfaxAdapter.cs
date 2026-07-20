using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Domain.Common;

namespace TryNextPost.Infrastructure.CourierAdapters
{
    public sealed class ShadowfaxAdapter : CourierAdapterBase
    {
        private readonly CourierProviderSettings _settings;

        public ShadowfaxAdapter(IOptions<CourierSettings> options, ILogger<ShadowfaxAdapter> logger)
            : base(logger)
        {
            _settings = options.Value.Shadowfax;
        }

        public override string CourierCode => CourierCodes.Shadowfax;

        protected override CourierProviderSettings Settings => _settings;
    }
}
