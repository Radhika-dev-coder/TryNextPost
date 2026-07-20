using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Domain.Common;

namespace TryNextPost.Infrastructure.CourierAdapters
{
    public sealed class BlueDartAdapter : CourierAdapterBase
    {
        private readonly CourierProviderSettings _settings;

        public BlueDartAdapter(IOptions<CourierSettings> options, ILogger<BlueDartAdapter> logger)
            : base(logger)
        {
            _settings = options.Value.BlueDart;
        }

        public override string CourierCode => CourierCodes.BlueDart;

        protected override CourierProviderSettings Settings => _settings;
    }
}
