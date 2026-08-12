using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Billing
{
    public class RateChartRowDto
    {
        public long CourierId { get; set; }
        public string CourierName { get; set; } = "";
        public string CourierCode { get; set; } = "";
        public string ServiceCode { get; set; } = "";
        public decimal WeightFromGrams { get; set; }
        public decimal WeightToGrams { get; set; }
        public string WeightLabel { get; set; } = "";
        public Dictionary<string, decimal?> ZoneRates { get; set; } = new();

        /// <summary>1 = Flat, 2 = Percentage (mirrors CodChargeType).</summary>
        public int CodChargeType { get; set; }

        /// <summary>Flat ₹ amount or percentage value from courier config.</summary>
        public decimal CodChargeValue { get; set; }

        /// <summary>
        /// Backward-compatible display amount when type is Flat; 0 when Percentage.
        /// </summary>
        public decimal CodChargeFlat { get; set; }

        public string CodLabel { get; set; } = "";
    }
}
