namespace TryNextPost.Application.DTO.Settlement
{
    public class CourierSettlementSummaryResponse
    {
        public long CourierId { get; set; }
        public string CourierCode { get; set; } = string.Empty;
        public string CourierName { get; set; } = string.Empty;
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public int ShipmentCount { get; set; }
        public decimal TotalCourierCost { get; set; }
        public decimal TotalSellerCharge { get; set; }
        public decimal TotalMargin { get; set; }
        public int AlreadySettledCount { get; set; }
        public int PendingSettlementCount { get; set; }
    }
}
