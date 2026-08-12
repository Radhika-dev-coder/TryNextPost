using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Application.DTO.Report
{
    public class ReportRequest
    {
        public ReportType ReportType { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string? Courier { get; set; }
        public string? State { get; set; }
        public string? ProductName { get; set; }
        public string? Channel { get; set; }
        public string? Zone { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;

        public List<string>? SelectedColumns { get; set; }
    }
}
