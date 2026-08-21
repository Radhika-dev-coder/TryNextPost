using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.Dtdc
{
    public sealed class DtdcZipcodeResp
    {
        [JsonPropertyName("ORGPIN")] public string? OrgPin { get; set; }
        [JsonPropertyName("DESTPIN")] public string? DestPin { get; set; }
        [JsonPropertyName("SERVFLAG")] public string? ServFlag { get; set; } // "Y" means serviceable
        [JsonPropertyName("SERV_COD")] public string? ServCod { get; set; }
    }
}
