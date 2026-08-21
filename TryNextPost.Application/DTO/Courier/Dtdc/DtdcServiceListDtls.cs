using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.Dtdc
{
    public sealed class DtdcServiceListDtls
    {
        [JsonPropertyName("CODE")] public string? Code { get; set; }          // e.g., "P7X", "D71"
        [JsonPropertyName("NAME")] public string? Name { get; set; }          // e.g., "B2C PRIORITY"
        [JsonPropertyName("TAT")] public string? Tat { get; set; }
    }
}
