namespace TryNextPost.Application.DTO.Wallet
{
    /// <summary>
    /// Razorpay Checkout success handler payload (order_id + payment_id + signature).
    /// </summary>
    public class VerifyPaymentRequest
    {
        public string RazorpayOrderId { get; set; } = string.Empty;
        public string RazorpayPaymentId { get; set; } = string.Empty;
        public string RazorpaySignature { get; set; } = string.Empty;
    }
}
