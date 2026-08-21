using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.Dtdc
{
    public sealed class DtdcPiecesDetail
    {
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("declared_value")] public string? DeclaredValue { get; set; }
        [JsonPropertyName("weight")] public string? Weight { get; set; }
        [JsonPropertyName("height")] public string? Height { get; set; }
        [JsonPropertyName("length")] public string? Length { get; set; }
        [JsonPropertyName("width")] public string? Width { get; set; }
    }
}
