namespace TryNextPost.Application.DTO.Wallet
{
    public class WalletBalanceResponse
    {
        public long WalletId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }
}
