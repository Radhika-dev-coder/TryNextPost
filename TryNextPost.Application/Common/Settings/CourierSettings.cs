namespace TryNextPost.Application.Common.Settings
{

    public class CourierSettings
    {
        public const string SectionName = "CourierSettings";

        public DelhiverySettings Delhivery { get; set; } = new();

        public BlueDartSettings BlueDart { get; set; } = new();

        public XpressbeesSettings Xpressbees { get; set; } = new();

        public DtdcSettings Dtdc { get; set; } = new();

        public EkartSettings Ekart { get; set; } = new();

        public IndiaPostSettings IndiaPost { get; set; } = new();

        public ShadowfaxSettings Shadowfax { get; set; } = new();


    }

    //public class CourierProviderSettings : ICourierSettings
    //{
    //    public string? BaseUrl { get; set; }
    //    public string? ApiKey { get; set; }
    //    public string? ApiSecret { get; set; }
    //    public string? SecretKey { get; set; }
    //    public string? ServiceabilityUrl { get; set; }
    //    public string AwbGenerationUrl { get; set; } = string.Empty;
    //    public string GetAwbSeriesUrl { get; set; } = string.Empty;
    //    public string? XBKey { get; set; }
    //    public string? BusinessUnit { get; set; }
    //    public string? AccountCode { get; set; }
    //    public bool Enabled { get; set; } = true;
    //    public string? TokenUrl { get; set; }
    //    public string? ForwardUrl { get; set; }
    //}

    //public class AmazonSettings : ICourierSettings
    //{
    //    public string? BaseUrl { get; set; }

    //    public string? ClientId { get; set; }

    //    public string? ClientSecret { get; set; }

    //    public string? RefreshToken { get; set; }

    //    public string? MarketplaceId { get; set; }

    //    public bool Enabled { get; set; } = true;
    //}
}
