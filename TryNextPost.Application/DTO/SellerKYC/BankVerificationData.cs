using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.SellerKYC
{
    public class BankVerificationData
    {
        [JsonPropertyName("client_id")]
        public string? ClientId { get; set; }

        [JsonPropertyName("account_number")]
        public string? AccountNumber { get; set; }

        [JsonPropertyName("account_exists")]
        public bool AccountExists { get; set; }

        [JsonPropertyName("upi_id")]
        public string? UpiId { get; set; }

        [JsonPropertyName("full_name")]
        public string? FullName { get; set; }

        [JsonPropertyName("imps_ref_no")]
        public string? ImpsRefNo { get; set; }

        [JsonPropertyName("remarks")]
        public string? Remarks { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("ifsc_details")]
        public IfscDetails? IfscDetails { get; set; }
    }
}
