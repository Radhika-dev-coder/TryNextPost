using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.Common.Settings
{
    public class XpressbeesSettings : ICourierSettings
    {
        public string? BaseUrl { get; set; }
        public string? ApiKey { get; set; }
        public string? ApiSecret { get; set; }
        public string? SecretKey { get; set; }
        public string? ServiceabilityUrl { get; set; }
        public string AwbGenerationUrl { get; set; } = string.Empty;
        public string GetAwbSeriesUrl { get; set; } = string.Empty;
        public string? XBKey { get; set; }
        public string? BusinessUnit { get; set; }
        public string? AccountCode { get; set; }
        public bool Enabled { get; set; } = true;
        public string? TokenUrl { get; set; }
        public string TrackingUrl { get; set; } = string.Empty;
        public string CancellationUrl { get; set; } = string.Empty;
        public string? ForwardUrl { get; set; }
        public string NdrUpdateUrl { get; set; } = string.Empty;

    }
}
