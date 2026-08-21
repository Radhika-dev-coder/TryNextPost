using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.Dtdc
{
    public sealed class DtdcCancelRequest
    {
        [JsonPropertyName("awb_number")]
        public string? AwbNumber { get; set; }

        [JsonPropertyName("cancel_reason")]
        public string CancelReason { get; set; }  = "Seller Cancelled Request";
    }
}
