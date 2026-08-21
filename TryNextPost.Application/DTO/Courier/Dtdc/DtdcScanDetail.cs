using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.Dtdc
{
    public sealed class DtdcScanDetail
    {
        [JsonPropertyName("status_code")] public string? StatusCode { get; set; }
        [JsonPropertyName("status_desc")] public string? StatusDesc { get; set; }
        [JsonPropertyName("location")] public string? Location { get; set; }
        [JsonPropertyName("date_time")] public DateTime? DateTime { get; set; }
    }
}
