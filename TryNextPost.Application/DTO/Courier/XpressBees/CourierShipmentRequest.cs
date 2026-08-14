using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.XpressBees
{
    public class CourierShipmentRequest
    {
        public long OrderId { get; set; }

        public long AddressId { get; set; }

        public long CourierId { get; set; }

        public string ServiceCode { get; set; } = null!;

        public string ServiceType { get; set; } = null!;
    }
}
