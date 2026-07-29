using TryNextPost.Domain.Enums;

namespace TryNextPost.Application.DTO.Courier
{
    public class CourierRateRequest
    {
        public string OriginPincode { get; set; } = string.Empty;
        public string DestinationPincode { get; set; } = string.Empty;
        public decimal WeightKg { get; set; }
        public decimal? LengthCm { get; set; }
        public decimal? BreadthCm { get; set; }
        public decimal? HeightCm { get; set; }
        public bool IsCod { get; set; }
        public decimal? CodAmount { get; set; }
        public string? PaymentMode { get; set; }

        /// <summary>From Courier master — used by stub/adapter COD fee resolution.</summary>
        public CodChargeType CodChargeType { get; set; } = CodChargeType.Flat;

        /// <summary>From Courier master — flat ₹ or percentage; no hardcoded fallback.</summary>
        public decimal CodChargeValue { get; set; }

        public bool SupportsCod { get; set; } = true;
    }
}
