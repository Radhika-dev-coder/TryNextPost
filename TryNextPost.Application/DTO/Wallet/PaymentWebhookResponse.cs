namespace TryNextPost.Application.DTO.Wallet
{
    public class PaymentWebhookResponse
    {
        public bool Processed { get; set; }
        public string Event { get; set; } = string.Empty;
        public string? GatewayOrderId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
