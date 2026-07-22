namespace TryNextPost.Application.DTO.Weight
{
    public class WeightDiscrepancyListItemResponse
    {
        public long WeightDiscrepancyId { get; set; }
        public DateTime WeightAppliedDate { get; set; }
        public string? AwbNumber { get; set; }
        public long? OrderId { get; set; }
        public string OrderRef { get; set; } = string.Empty;
        public decimal EnteredWeightGrams { get; set; }
        public decimal AppliedWeightGrams { get; set; }
        public decimal WeightCharges { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? CourierName { get; set; }
        public long? CourierId { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? DisputeRemarks { get; set; }
    }
}
