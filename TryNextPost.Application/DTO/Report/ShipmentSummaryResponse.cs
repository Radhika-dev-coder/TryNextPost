using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Report
{
    public class ShipmentSummaryResponse
    {
        public int Booked { get; set; }
        public int ShipmentPicked { get; set; }
        public int Delivered { get; set; }
        public int RtoInitiated { get; set; }
        public int RtoDelivered { get; set; }
        public int RtoAcknowledged { get; set; }
    }
}
