namespace TryNextPost.Application.Common.Settings
{

    public class CourierSettings
    {
        public const string SectionName = "CourierSettings";

        public CourierProviderSettings Delhivery { get; set; } = new();
        public CourierProviderSettings BlueDart { get; set; } = new();
        public CourierProviderSettings Xpressbees { get; set; } = new();
        public CourierProviderSettings Dtdc { get; set; } = new();
        public CourierProviderSettings Ekart { get; set; } = new();
        public CourierProviderSettings IndiaPost { get; set; } = new();
        public CourierProviderSettings Shadowfax { get; set; } = new();
    }

    public class CourierProviderSettings
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
        public string? ForwardUrl { get; set; }
    }
}
