using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Report
{
    public class DailySummaryRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
