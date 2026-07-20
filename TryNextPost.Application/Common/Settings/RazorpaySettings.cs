namespace TryNextPost.Application.Common.Settings
{
    /// <summary>
    /// Razorpay Test/Live keys. Leave empty in appsettings; set via User Secrets or environment.
    /// Never commit real KeySecret / WebhookSecret.
    /// </summary>
    public class RazorpaySettings
    {
        public const string SectionName = "Razorpay";

        public string KeyId { get; set; } = string.Empty;
        public string KeySecret { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;

        /// <summary>Defaults to https://api.razorpay.com/v1</summary>
        public string BaseUrl { get; set; } = "https://api.razorpay.com/v1";
    }
}
