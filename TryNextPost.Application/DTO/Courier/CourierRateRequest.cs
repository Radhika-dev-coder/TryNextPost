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
        public CodChargeType CodChargeType { get; set; } = CodChargeType.Flat;
        public decimal CodChargeValue { get; set; }
        public bool SupportsCod { get; set; } = true;
        public int TotalQuantity { get; set; } = 1;

    }
}
