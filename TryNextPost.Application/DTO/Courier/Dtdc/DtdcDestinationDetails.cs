using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.Dtdc
{
    public class DtdcDestinationDetails
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("alternate_phone")]
        public string? AlternatePhone { get; set; }

        [JsonPropertyName("address_line_1")]
        public string? AddressLine1 { get; set; }

        [JsonPropertyName("address_line_2")]
        public string? AddressLine2 { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("pincode")]
        public string? Pincode { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("latitude")]
        public string? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public string? Longitude { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }
    }
}
