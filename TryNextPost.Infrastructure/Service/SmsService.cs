using Microsoft.Extensions.Options;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Application.IServices.Interface;

namespace TryNextPost.Infrastructure.Service
{
    public class SmsService : ISmsService
    {
        private readonly HttpClient _httpClient;
        private readonly SmsSettings _smsSettings;

        public SmsService(HttpClient httpClient, IOptions<SmsSettings> smsSettings)
        {
            _httpClient = httpClient;
            _smsSettings = smsSettings.Value;
        }

        public async Task SendOtpSms(string mobile, string otp)
        {
            if (string.IsNullOrWhiteSpace(_smsSettings.ApiKey))
                throw new InvalidOperationException("SmsSettings:ApiKey is not configured.");

            // iCPaaS expects a 10-digit Indian mobile number (no country code).
            if (mobile.StartsWith("91") && mobile.Length == 12)
                mobile = mobile[2..];

            var text = _smsSettings.TemplateText.Replace("{#num#}", otp);

            var url =
                $"{_smsSettings.BaseUrl.TrimEnd('/')}/api/v1/sms/sendsms" +
                $"?ApiKey={Uri.EscapeDataString(_smsSettings.ApiKey)}" +
                $"&Number={Uri.EscapeDataString(mobile)}" +
                $"&SenderId={Uri.EscapeDataString(_smsSettings.SenderId)}" +
                $"&Text={Uri.EscapeDataString(text)}" +
                $"&DLTTemplateId={Uri.EscapeDataString(_smsSettings.DLTTemplateId)}";

            var response = await _httpClient.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception("SMS sending failed: " + result);
        }
    }
}
