using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Infrastructure.CourierAdapters
{
    public sealed class EkartAdapter : CourierAdapterBase
    {
        private readonly EkartSettings _settings;

        public EkartAdapter(IOptions<CourierSettings> options, ILogger<EkartAdapter> logger, IOrderRepository orderRepository)
            : base(logger, orderRepository)
        {
            _settings = options.Value.Ekart;
        }
        public override string CourierCode =>CourierCodes.Ekart;
        protected override bool IsConfigured =>
    _settings.Enabled &&
    !string.IsNullOrWhiteSpace(_settings.BaseUrl) &&
    !string.IsNullOrWhiteSpace(_settings.ApiKey) &&
    !string.IsNullOrWhiteSpace(_settings.ApiSecret);


        public override async Task<bool> RequestNdrReAttemptAsync(string awbNumber, string actionType, string remarks, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Ekart NDR workflow not integrated yet.");
        }


    }
}
