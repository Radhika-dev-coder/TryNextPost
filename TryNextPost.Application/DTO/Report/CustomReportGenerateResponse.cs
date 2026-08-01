using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Report
{
    public class CustomReportGenerateResponse
    {
        public long ExportHistoryId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public int RowCount { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
    }
}
