using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Report
{
    public class ChannelSummaryResponse
    {
        public string ChannelName { get; set; }
        public int TotalOrders { get; set; }
        public int TotalShipments { get; set; }
        public int Booked { get; set; }
        public int PendingPickup { get; set; }
        public int InTransit { get; set; }
        public int Delivered { get; set; }
        public int RTO { get; set; }
    }
}
