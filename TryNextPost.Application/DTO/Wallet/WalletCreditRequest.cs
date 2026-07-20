namespace TryNextPost.Application.DTO.Wallet
{
    public class WalletCreditRequest
    {
        /// <summary>
        /// Target wallet owner (e.g. Seller TNP-000002). Required for SuperAdmin manual top-up.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string? ReferenceId { get; set; }
    }
}
