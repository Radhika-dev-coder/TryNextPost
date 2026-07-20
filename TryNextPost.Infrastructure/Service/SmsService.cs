using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Application.IServices.Interface;
using static System.Net.WebRequestMethods;

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
            // India format fix
            if (!mobile.StartsWith("91"))
                mobile = "91" + mobile;

            var message = $"Use this OTP {otp} for log in purpose only. Do not share it with anyone";

            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("username", _smsSettings.Username),
            new KeyValuePair<string, string>("password", _smsSettings.Password),
            new KeyValuePair<string, string>("to", mobile),
            new KeyValuePair<string, string>("from", _smsSettings.SenderId),
            new KeyValuePair<string, string>("templateId", _smsSettings.TemplateId),
            new KeyValuePair<string, string>("message", message)
        });

            var response = await _httpClient.PostAsync(_smsSettings.BaseUrl, content);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("SMS sending failed: " + result);
            }
        }
    }
}
