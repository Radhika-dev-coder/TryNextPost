using TryNextPost.Application.DTO.Payment;

namespace TryNextPost.Application.IServices.Interface.IPayment
{
    public interface IRazorpayPaymentGateway
    {
        /// <summary>
        /// Creates a Razorpay order via POST /v1/orders (Basic Auth KeyId:KeySecret).
        /// Throws InvalidOperationException when credentials are missing.
        /// </summary>
        Task<RazorpayCreateOrderResult> CreateOrderAsync(
            int amountPaise,
            string receipt,
            IDictionary<string, string>? notes = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies X-Razorpay-Signature = HMAC-SHA256(rawBody, WebhookSecret).
        /// </summary>
        bool VerifyWebhookSignature(string rawBody, string signature);

        /// <summary>
        /// Verifies Checkout signature = HMAC-SHA256(orderId|paymentId, KeySecret).
        /// </summary>
        bool VerifyPaymentSignature(string orderId, string paymentId, string signature);
    }
}
