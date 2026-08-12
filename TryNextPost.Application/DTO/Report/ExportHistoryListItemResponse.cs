using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Report
{
    public class ExportHistoryListItemResponse
    {
        public long ExportHistoryId { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string SelectedFields { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public int RowCount { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
