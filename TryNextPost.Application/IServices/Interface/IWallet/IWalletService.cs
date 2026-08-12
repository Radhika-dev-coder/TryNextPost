using TryNextPost.Application.DTO.Wallet;

namespace TryNextPost.Application.IServices.Interface.IWallet
{
    public interface IWalletService
    {
        Task<WalletBalanceResponse> GetOrCreateBalanceAsync(string userId);

        Task<WalletBalanceResponse> GetSellerWalletBalanceAsync(string userId);

        Task<WalletBalanceResponse> CreditAsync(string userId, WalletCreditRequest request, string? performedBy = null);

        Task<WalletBalanceResponse> DebitForShipmentAsync(
            string userId,
            decimal amount,
            long shipmentId,
            string? awbNumber,
            string? performedBy);

        /// <summary>
        /// Credits ChargedAmount back to the seller wallet after cancel.
        /// Idempotent via TxnReference SHIP-REFUND-{shipmentId}.
        /// </summary>
        Task<WalletBalanceResponse> CreditForShipmentRefundAsync(
            string userId,
            decimal amount,
            long shipmentId,
            string? awbNumber,
            string? performedBy);

        /// <summary>
        /// Debits WeightCharges for accepted discrepancy.
        /// Idempotent via TxnReference WD-ACCEPT-{weightDiscrepancyId}.
        /// No-op when amount is 0.
        /// </summary>
        Task<WalletBalanceResponse> DebitForWeightDiscrepancyAsync(
            long sellerId,
            decimal amount,
            long weightDiscrepancyId,
            string? awbNumber,
            string performedBy);

        Task<WalletTransactionListResponse> GetTransactionsAsync(
            string userId,
            bool isSuperAdmin,
            WalletTransactionFilterRequest filter);

        Task<WalletRechargeResponse> CreateRechargeAsync(string userId, WalletRechargeRequest request);

        Task<PaymentWebhookResponse> HandleWebhookAsync(string rawBody, string? signature);

        Task<VerifyPaymentResponse> VerifyPaymentAsync(string userId, VerifyPaymentRequest request);
    }
}
