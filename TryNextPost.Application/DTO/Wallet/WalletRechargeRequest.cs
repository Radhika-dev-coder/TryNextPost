namespace TryNextPost.Application.DTO.Wallet
{
    public class WalletRechargeRequest
    {
        /// <summary>Amount in INR (rupees). Must be greater than zero.</summary>
        public decimal Amount { get; set; }
    }
}
