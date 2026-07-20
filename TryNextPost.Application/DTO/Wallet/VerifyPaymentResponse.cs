namespace TryNextPost.Application.DTO.Wallet
{
    public class VerifyPaymentResponse
    {
        public long PaymentOrderId { get; set; }
        public string GatewayOrderId { get; set; } = string.Empty;
        public string? GatewayPaymentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal WalletBalance { get; set; }
        public bool AlreadyProcessed { get; set; }
    }
}
