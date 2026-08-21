using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.Dtdc
{
    public class DtdcServiceabilityDetailsResult
    {
        public bool IsPickupServiceable { get; set; }
        public bool IsDeliveryServiceable { get; set; }
        public bool IsOverallServiceable { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
