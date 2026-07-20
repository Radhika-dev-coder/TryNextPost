using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Domain.Common;

namespace TryNextPost.Infrastructure.CourierAdapters
{
    public sealed class DtdcAdapter : CourierAdapterBase
    {
        private readonly CourierProviderSettings _settings;

        public DtdcAdapter(IOptions<CourierSettings> options, ILogger<DtdcAdapter> logger)
            : base(logger)
        {
            _settings = options.Value.Dtdc;
        }

        public override string CourierCode => CourierCodes.Dtdc;

        protected override CourierProviderSettings Settings => _settings;
    }
}
