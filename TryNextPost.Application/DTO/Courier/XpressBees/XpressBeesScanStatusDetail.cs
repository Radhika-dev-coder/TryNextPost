using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.XpressBees
{
    public sealed class XpressBeesScanStatusDetail
    {
        [JsonPropertyName("AWBNO")] public string? AwbNo { get; set; }
        [JsonPropertyName("OriginLocation")] public string? OriginLocation { get; set; }
        [JsonPropertyName("CurrentLocation")] public string? CurrentLocation { get; set; }
        [JsonPropertyName("FinalDestinationName")] public string? FinalDestinationName { get; set; }
        [JsonPropertyName("StatusCode")] public string? StatusCode { get; set; } 
        [JsonPropertyName("Status")] public string? Status { get; set; }
        [JsonPropertyName("StatusDateTime")] public string? StatusDateTime { get; set; } 
        [JsonPropertyName("Remark")] public string? Remark { get; set; }
    }
}
