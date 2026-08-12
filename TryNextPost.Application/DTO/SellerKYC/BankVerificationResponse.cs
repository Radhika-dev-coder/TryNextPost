using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.SellerKYC
{
    public class BankVerificationResponse
    {
        [JsonPropertyName("data")]
        public BankVerificationData? Data { get; set; }

        [JsonPropertyName("status_code")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message_code")]
        public string? MessageCode { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
