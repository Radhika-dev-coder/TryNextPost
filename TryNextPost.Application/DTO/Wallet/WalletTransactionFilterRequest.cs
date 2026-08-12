namespace TryNextPost.Application.DTO.Wallet
{
    public class WalletTransactionFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        /// <summary>all | credit | debit</summary>
        public string? TxnType { get; set; } = "all";

        /// <summary>Search in description / reference (AWB-ish text).</summary>
        public string? Search { get; set; }

        /// <summary>Optional SuperAdmin scope: view another seller's passbook.</summary>
        public long? SellerId { get; set; }
    }
}
