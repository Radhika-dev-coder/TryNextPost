using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Report
{
    public class TopNdrReasonsResponse
    {
        public string Reason { get; set; } = string.Empty;
        public int TotalCount { get; set; }
    }
}
