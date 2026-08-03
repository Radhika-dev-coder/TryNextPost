using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Report
{
    public class DailySummaryResponse
    {
        public DateTime Date { get; set; }
        public int ShipmentPicked { get; set; }
        public int InTransit { get; set; }
        public int Exception { get; set; }
        public int Delivered { get; set; }

        public int RTOInTransit { get; set; }
        public int RTODelivered { get; set; }
    }
}
