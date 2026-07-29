using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Billing
{
    public class RateChartRequest
    {
        public long? CourierId { get; set; }
        public string? ServiceCode { get; set; }   
        public decimal? WeightGrams { get; set; }
        public string? Direction { get; set; }    
        public int? FromZoneId { get; set; }
    }
}
