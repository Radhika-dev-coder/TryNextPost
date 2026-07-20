using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Domain.Common;

namespace TryNextPost.Infrastructure.CourierAdapters
{
    public sealed class IndiaPostAdapter : CourierAdapterBase
    {
        private readonly CourierProviderSettings _settings;

        public IndiaPostAdapter(IOptions<CourierSettings> options, ILogger<IndiaPostAdapter> logger)
            : base(logger)
        {
            _settings = options.Value.IndiaPost;
        }

        public override string CourierCode => CourierCodes.IndiaPost;

        protected override CourierProviderSettings Settings => _settings;
    }
}
