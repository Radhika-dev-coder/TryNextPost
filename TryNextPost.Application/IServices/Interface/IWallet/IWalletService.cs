using TryNextPost.Application.DTO.Wallet;

namespace TryNextPost.Application.IServices.Interface.IWallet
{
    public interface IWalletService
    {
        Task<WalletBalanceResponse> GetOrCreateBalanceAsync(string userId);

        /// <summary>
        /// Credits <paramref name="userId"/> wallet. <paramref name="performedBy"/> is usually SuperAdmin id.
        /// </summary>
        Task<WalletBalanceResponse> CreditAsync(string userId, WalletCreditRequest request, string? performedBy = null);

        /// <summary>
        /// Debits wallet after a successful balance check. Creates wallet (0 balance) if missing.
        /// </summary>
        Task DebitForShipmentAsync(
            string userId,
            decimal amount,
            long shipmentId,
            string? awbNumber,
            string? performedBy);

        /// <summary>
        /// Creates a Razorpay order and a Pending WalletRecharge row.
        /// </summary>
        Task<WalletRechargeResponse> CreateRechargeAsync(string userId, WalletRechargeRequest request);

        /// <summary>
        /// Verifies Razorpay webhook signature and credits wallet once on payment.captured.
        /// </summary>
        Task<PaymentWebhookResponse> HandleWebhookAsync(string rawBody, string? signature);

        /// <summary>
        /// Optional frontend return path: verify Checkout signature and credit if not already Paid.
        /// </summary>
        Task<VerifyPaymentResponse> VerifyPaymentAsync(string userId, VerifyPaymentRequest request);
    }
}
