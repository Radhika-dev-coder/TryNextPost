using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.XpressBees
{
    public sealed class XpressBeesTrackingRootResponse
    {
        [JsonPropertyName("ReturnCode")] public int ReturnCode { get; set; }
        [JsonPropertyName("ReturnMessage")] public string? ReturnMessage { get; set; }
        [JsonPropertyName("CurrentShipmentStatus")] public List<XpressBeesScanStatusDetail>? CurrentShipmentStatus { get; set; }
    }
}
