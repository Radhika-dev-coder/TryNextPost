namespace TryNextPost.Application.DTO.RateCard
{
    public class RateQuoteDto
    {
        public string ServiceCode { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public decimal SellerCharge { get; set; }
        public decimal CourierCost { get; set; }
        public decimal Margin { get; set; }
        public decimal CodCharge { get; set; }
        public decimal TotalCharge { get; set; }
        public int EstimatedDays { get; set; }
        public bool FromRateCard { get; set; }
        public string? OriginZoneCode { get; set; }
        public string? DestinationZoneCode { get; set; }
        public decimal ChargeableWeightGrams { get; set; }
    }
}
