using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Common.Report
{
    public class DailySummaryData
    {
        public DateTime Date { get; set; }
        public int ShipmentPicked { get; set; }
        public int InTransit { get; set; }
        public int Exception { get; set; }
        public int Delivered { get; set; }
        public int RTOInTransit { get; set; }
        public int RTODelivered { get; set; }

        public int RtoInTransit { get; set; }

        public int RtoDelivered { get; set; }
    }
}
