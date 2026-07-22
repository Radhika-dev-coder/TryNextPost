namespace TryNextPost.Application.DTO.Shipment
{
    public class ConfirmShipmentResponse
    {
        public long ShipmentId { get; set; }
        public long OrderId { get; set; }
        public string? AwbNumber { get; set; }
        public long CourierId { get; set; }
        public string CourierCode { get; set; } = string.Empty;
        public string? CourierName { get; set; }
        public string? ServiceCode { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public decimal ChargedAmount { get; set; }
        public decimal WalletBalanceAfterDebit { get; set; }
        public bool IsStub { get; set; }
        public string? LabelUrl { get; set; }
        public string? CourierReference { get; set; }
        public string? Message { get; set; }
    }
}
