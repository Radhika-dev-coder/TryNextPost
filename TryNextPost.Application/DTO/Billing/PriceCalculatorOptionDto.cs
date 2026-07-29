using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Billing
{
    public class PriceCalculatorOptionDto
    {
        public long CourierId { get; set; }
        public string CourierCode { get; set; } = string.Empty;
        public string CourierName { get; set; } = string.Empty;
        public string ServiceCode { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public decimal SellerCharge { get; set; }
        public decimal CodCharge { get; set; }
        public decimal TotalCharge { get; set; }
        public int EstimatedDays { get; set; }
        public string? OriginZoneCode { get; set; }
        public string? DestinationZoneCode { get; set; }
        public decimal ChargeableWeightGrams { get; set; }
    }
}
