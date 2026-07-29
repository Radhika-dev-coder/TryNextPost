using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Billing
{
    public class PriceCalculatorResponse
    {
        public string OriginPincode { get; set; } = string.Empty;
        public string DestinationPincode { get; set; } = string.Empty;
        public decimal ChargeableWeightGrams { get; set; }
        public List<PriceCalculatorOptionDto> Rates { get; set; } = new();
    }
}
