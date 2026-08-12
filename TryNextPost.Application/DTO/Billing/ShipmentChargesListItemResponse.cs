namespace TryNextPost.Application.DTO.Billing
{
    public class ShipmentChargesListItemResponse
    {
        public long ShipmentChargesId { get; set; }
        public long ShipmentId { get; set; }
        public DateTime? ShipmentCreated { get; set; }
        public string Courier { get; set; } = string.Empty;
        public string? AwbNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal FreightCharges { get; set; }
        public decimal CodCharges { get; set; }
        public decimal EnteredWeightKg { get; set; }
        public decimal AppliedWeightKg { get; set; }
        public decimal ExtraWeightCharges { get; set; }
        public decimal RtoCharges { get; set; }
        public decimal CodChargeReversed { get; set; }
        public decimal RtoExtraWeightCharges { get; set; }
        public decimal ShipmentInsuranceCharges { get; set; }
        public decimal TotalCharges { get; set; }
    }
}
