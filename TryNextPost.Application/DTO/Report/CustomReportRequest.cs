using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Report
{
    public class CustomReportRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        ///  ["orderNumber","awb","warehouseName"] — CustomReportFieldKeys se match
        public List<string> Fields { get; set; } = new();
        /// "csv" or "xlsx" — default csv
        public string Format { get; set; } = "csv";
    }
}
