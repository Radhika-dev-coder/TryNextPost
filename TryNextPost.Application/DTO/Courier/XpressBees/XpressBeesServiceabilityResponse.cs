using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.XpressBees
{
    public class XpressBeesServiceabilityResponse
    {
        [JsonPropertyName("ReturnCode")]
        public int ReturnCode { get; set; }

        [JsonPropertyName("ReturnMessage")]
        public string ReturnMessage { get; set; } = string.Empty;

        [JsonPropertyName("ServicablePincodeDetails")]
        public List<XpressBeesPincodeDetail> ServicablePincodeDetails { get; set; } = new();
    }
}
