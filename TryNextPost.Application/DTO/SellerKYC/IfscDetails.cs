using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.SellerKYC
{
    public class IfscDetails
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("ifsc")]
        public string? Ifsc { get; set; }

        [JsonPropertyName("micr")]
        public string? Micr { get; set; }

        [JsonPropertyName("iso3166")]
        public string? Iso3166 { get; set; }

        [JsonPropertyName("swift")]
        public string? Swift { get; set; }

        [JsonPropertyName("bank")]
        public string? Bank { get; set; }

        [JsonPropertyName("bank_code")]
        public string? BankCode { get; set; }

        [JsonPropertyName("bank_name")]
        public string? BankName { get; set; }

        [JsonPropertyName("branch")]
        public string? Branch { get; set; }

        [JsonPropertyName("centre")]
        public string? Centre { get; set; }

        [JsonPropertyName("district")]
        public string? District { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("contact")]
        public string? Contact { get; set; }

        [JsonPropertyName("imps")]
        public bool Imps { get; set; }

        [JsonPropertyName("rtgs")]
        public bool Rtgs { get; set; }

        [JsonPropertyName("neft")]
        public bool Neft { get; set; }

        [JsonPropertyName("upi")]
        public bool Upi { get; set; }

        [JsonPropertyName("micr_check")]
        public bool MicrCheck { get; set; }
    }
}
