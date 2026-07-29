using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Billing
{
    public class PriceCalculatorRequest
    {
        public string OriginPincode { get; set; } = string.Empty;
        public string DestinationPincode { get; set; } = string.Empty;
        public decimal WeightGrams { get; set; }
        public decimal? VolumetricWeightGrams { get; set; }
        public bool IsCod { get; set; }

        /// <summary>
        /// Seller collectable / COD order amount. Required for percentage-based courier COD fees.
        /// </summary>
        public decimal? CodAmount { get; set; }

        public long? CourierId { get; set; }     
        public string? ServiceCode { get; set; }
    }
}
