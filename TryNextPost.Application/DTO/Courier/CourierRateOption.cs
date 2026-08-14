using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier
{
    public class CourierRateOption
    {
        public string ServiceName { get; set; } = string.Empty;
        public string? ServiceCode { get; set; }
        public decimal TotalCharge { get; set; }
        public decimal? CodCharge { get; set; }
        public int EstimatedDays { get; set; }
        public bool IsStub { get; set; }
    }
}
    