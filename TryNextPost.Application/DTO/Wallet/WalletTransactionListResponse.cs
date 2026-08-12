namespace TryNextPost.Application.DTO.Wallet
{
    public class WalletTransactionListResponse
    {
        public decimal WalletBalance { get; set; }
        public List<WalletTransactionListItemResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
