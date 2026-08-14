using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.XpressBees
{
    public class XpressBeesPincodeDetail
    {
     //   public string RowId { get; set; } = string.Empty;

        [JsonPropertyName("businessunit")]
        public string BusinessUnit { get; set; } = string.Empty;

        [JsonPropertyName("businessflow")]
        public string BusinessFlow { get; set; } = string.Empty;

        [JsonPropertyName("businessservice")]
        public string BusinessService { get; set; } = string.Empty;

        [JsonPropertyName("pincode")]
        public int Pincode { get; set; }

        [JsonPropertyName("HubName")]
        public string HubName { get; set; } = string.Empty;

        [JsonPropertyName("processcode")]
        public string ProcessCode { get; set; } = string.Empty;

        [JsonPropertyName("rtoprocesscode")]
        public string? RtoProcessCode { get; set; }

        [JsonPropertyName("statename")]
        public string StateName { get; set; } = string.Empty;

        [JsonPropertyName("cityname")]
        public string CityName { get; set; } = string.Empty;
    }
}
