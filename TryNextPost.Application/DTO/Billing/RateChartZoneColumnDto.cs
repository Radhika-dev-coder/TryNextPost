using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Billing
{
    public class RateChartZoneColumnDto
    {
        public int ZoneId { get; set; }
        public string ZoneCode { get; set; } = "";
        public string ZoneLabel { get; set; } = "";
    }
}
