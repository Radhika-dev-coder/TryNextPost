using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.Dtdc
{
    public sealed class DtdcBookingRequest
    {
        [JsonPropertyName("consignments")]
        public List<DtdcConsignment> Consignments { get; set; } = new();
    }
}
