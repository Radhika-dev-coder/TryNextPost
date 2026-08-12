using TryNextPost.Application.DTO.Payment;

namespace TryNextPost.Application.IServices.Interface.IPayment
{
    public interface IRazorpayPaymentGateway
    {

        Task<RazorpayCreateOrderResult> CreateOrderAsync(
            int amountPaise,
            string receipt,
            IDictionary<string, string>? notes = null,
            CancellationToken cancellationToken = default);

        bool VerifyWebhookSignature(string rawBody, string signature);

        bool VerifyPaymentSignature(string orderId, string paymentId, string signature);
    }
}
