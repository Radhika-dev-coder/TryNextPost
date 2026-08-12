using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.SellerKYC
{
    public class PanData
    {
        [JsonPropertyName("client_id")]
        public string? ClientId { get; set; }

        [JsonPropertyName("pan_number")]
        public string? PanNumber { get; set; }

        [JsonPropertyName("full_name")]
        public string? FullName { get; set; }

        [JsonPropertyName("full_name_split")]
        public List<string>? FullNameSplit { get; set; }

        [JsonPropertyName("masked_aadhaar")]
        public string? MaskedAadhaar { get; set; }

        [JsonPropertyName("address")]
        public PanAddress? Address { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("phone_number")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        [JsonPropertyName("dob")]
        public string? Dob { get; set; }

        [JsonPropertyName("aadhaar_linked")]
        public bool? AadhaarLinked { get; set; }

        [JsonPropertyName("dob_verified")]
        public bool DobVerified { get; set; }

        [JsonPropertyName("dob_check")]
        public bool DobCheck { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("message_code")]
        public string? MessageCode { get; set; }

        [JsonPropertyName("less_info")]
        public bool LessInfo { get; set; }
    }
}
