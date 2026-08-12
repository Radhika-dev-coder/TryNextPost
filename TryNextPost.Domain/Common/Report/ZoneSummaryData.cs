using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Common.Report
{
    public class ZoneSummaryData
    {
        public string ZoneName { get; set; }
        public int TotalShipment { get; set; }
        public int Booked { get; set; }
        public int PendingPickup { get; set; }
        public int InTransit { get; set; }
        public int Delivered { get; set; }
        public int RTO { get; set; }
    }
}
