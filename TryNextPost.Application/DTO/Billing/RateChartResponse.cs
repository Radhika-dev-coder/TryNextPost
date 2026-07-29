using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Billing
{
    public class RateChartResponse
    {
        public int? FromZoneId { get; set; }
        public string? FromZoneCode { get; set; }
        public List<RateChartZoneColumnDto> Zones { get; set; } = new();
        public List<RateChartRowDto> Rows { get; set; } = new();

        /// <summary>Optional informational message (e.g. RTO chart not configured).</summary>
        public string? InfoMessage { get; set; }
    }
}
