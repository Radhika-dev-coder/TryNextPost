namespace TryNextPost.Application.Common.Settings
{
    public class SmsSettings
    {
        public string BaseUrl { get; set; } = "https://icpaas.in";
        public string ApiKey { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string DLTTemplateId { get; set; } = string.Empty;
        public string TemplateText { get; set; } = string.Empty;
    }
}
