using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.Dtdc
{
    public sealed class DtdcPincodeResponse
    {
        [JsonPropertyName("ZIPCODE_RESP")]
        public List<DtdcZipcodeResp>? ZipcodeResp { get; set; }

        [JsonPropertyName("SERV_LIST_DTLS")]
        public List<DtdcServiceListDtls>? ServListDtls { get; set; }
    }
}
