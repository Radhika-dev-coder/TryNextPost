namespace TryNextPost.Application.DTO.Shipment
{
    public class CancelShipmentResponse
    {
        public long ShipmentId { get; set; }
        public string? AwbNumber { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public decimal RefundedAmount { get; set; }
        public decimal WalletBalanceAfterRefund { get; set; }
        public bool AlreadyCancelled { get; set; }
        public bool IsStub { get; set; }
        public string? Message { get; set; }
    }
}
