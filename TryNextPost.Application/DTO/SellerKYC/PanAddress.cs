using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.SellerKYC
{
    public class PanAddress
    {
        [JsonPropertyName("line_1")]
        public string? Line1 { get; set; }

        [JsonPropertyName("line_2")]
        public string? Line2 { get; set; }

        [JsonPropertyName("street_name")]
        public string? StreetName { get; set; }

        [JsonPropertyName("zip")]
        public string? Zip { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("full")]
        public string? Full { get; set; }
    }
}
