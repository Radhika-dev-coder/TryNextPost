using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Common.Report
{
    public class ReportFilter
    {
        public ReportType ReportType { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string? Courier { get; set; }
        public string? State { get; set; }
        public string? ProductName { get; set; }
        public string? Channel { get; set; }
        public string? Zone { get; set; }
    }
}
