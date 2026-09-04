using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.Common.Settings
{
    public class DtdcSettings :ICourierSettings
    {
        public string? BaseUrl { get; set; }

        public string? BookingUrl { get; set; }

        public string? ApiKey { get; set; }

        public string? ApiSecret { get; set; }

        public string? AccountCode { get; set; }

        public string TrackingUrl { get; set; } = string.Empty;
        public string CancellationUrl { get; set; } = string.Empty;

        public string LabelUrl { get; set; } = string.Empty;

        public bool Enabled { get; set; }
        public string? PincodeUrl { get; set; }
        public string? TrackingUsername { get; set; }
        public string? TrackingToken { get; set; }
        public string? ServiceTypeId { get; set; }
        public string NdrUpdateUrl { get; set; } = string.Empty;
        public string NdrUsername { get; set; } = string.Empty;
        public string NdrPassword { get; set; } = string.Empty;

        public string RateCalculatorUrl { get; set; } = string.Empty;

    }
}
