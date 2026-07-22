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

        Task<WalletRechargeResponse> CreateRechargeAsync(string userId, WalletRechargeRequest request);

        Task<PaymentWebhookResponse> HandleWebhookAsync(string rawBody, string? signature);

        Task<VerifyPaymentResponse> VerifyPaymentAsync(string userId, VerifyPaymentRequest request);
    }
}
