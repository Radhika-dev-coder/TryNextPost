using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;


namespace TryNextPost.Application.DTO.Courier.Dtdc
{
    public class DtdcPincodeRequest
    {
        [JsonPropertyName("orgPincode")]
        public string OrgPincode { get; set; } = string.Empty;

        [JsonPropertyName("desPincode")]
        public string DesPincode { get; set; } = string.Empty;
    }
}
