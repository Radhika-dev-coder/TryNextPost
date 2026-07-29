using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.IRepository
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetByUserIdAsync(string userId);
        Task<Wallet?> GetBySellerIdAsync(long sellerId);
        Task<Wallet?> GetByIdAsync(long walletId);
        Task AddAsync(Wallet wallet);
        Task UpdateAsync(Wallet wallet);
        Task AddTransactionAsync(Transaction transaction);
        Task<Transaction?> GetSuccessfulByTxnReferenceAsync(string txnReference);

        Task<(List<Transaction> Items, int TotalCount)> GetTransactionsFilteredAsync(
            long walletId,
            TransactionType? txnType,
            DateTime? fromDate,
            DateTime? toDate,
            string? search,
            int page,
            int pageSize);

        /// <summary>
        /// Newest-first successful transactions used to compute closing balances for a page.
        /// </summary>
        Task<List<Transaction>> GetSuccessfulTransactionsNewestFirstAsync(long walletId, int take);

        /// <summary>
        /// Newest-first successful txns from current back through <paramref name="untilCreatedAt"/> /
        /// <paramref name="untilTxnId"/> (inclusive). Used for page-scoped closing balances.
        /// </summary>
        Task<List<Transaction>> GetSuccessfulTransactionsNewestFirstUntilAsync(
            long walletId,
            DateTime untilCreatedAt,
            long untilTxnId);

        Task SaveChangesAsync();
    }
}
