using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities.Report
{
    public class ExportHistory : BaseDbModel
    {
        [Key]
        public long ExportHistoryId { get; set; }
        public long SellerId { get; set; }
        public Seller? Seller { get; set; }
        [MaxLength(100)]
        public string ReportType { get; set; } = "CustomReport";
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        
        [MaxLength(2000)]
        public string SelectedFields { get; set; } = string.Empty;
        public ExportHistoryStatus Status { get; set; } = ExportHistoryStatus.Pending;
        [MaxLength(260)]
        public string FileName { get; set; } = string.Empty;
       
        [MaxLength(500)]
        public string? FilePath { get; set; }
        public int RowCount { get; set; }
        [MaxLength(500)]
        public string? ErrorMessage { get; set; }
    }
}
