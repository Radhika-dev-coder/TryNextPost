namespace TryNextPost.Application.DTO.Wallet
{
    /// <summary>
    /// Returned to the seller frontend to open Razorpay Checkout.
    /// </summary>
    public class WalletRechargeResponse
    {
        public long PaymentOrderId { get; set; }
        public string GatewayOrderId { get; set; } = string.Empty;
        public string KeyId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int AmountInPaise { get; set; }
        public string Currency { get; set; } = "INR";
        public string Receipt { get; set; } = string.Empty;
    }
}
