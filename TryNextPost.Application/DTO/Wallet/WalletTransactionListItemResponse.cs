namespace TryNextPost.Application.DTO.Wallet
{
    public class WalletTransactionListItemResponse
    {
        public long TxnId { get; set; }
        public DateTime Date { get; set; }
        public string TxnType { get; set; } = string.Empty;
        public int TxnTypeCode { get; set; }
        public string? RefNo { get; set; }
        public string? TransactionId { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public decimal ClosingBalance { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
