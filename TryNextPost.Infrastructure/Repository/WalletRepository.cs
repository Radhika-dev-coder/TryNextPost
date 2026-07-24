using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class WalletRepository : IWalletRepository
    {
        private readonly AppDbContext _context;

        public WalletRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Wallet?> GetByUserIdAsync(string userId)
        {
            return await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId && w.IsActive == true);
        }

        public async Task<Wallet?> GetBySellerIdAsync(long sellerId)
        {
            return await _context.Wallets
                .FirstOrDefaultAsync(w => w.SellerId == sellerId && w.IsActive == true);
        }

        public async Task<Wallet?> GetByIdAsync(long walletId)
        {
            return await _context.Wallets
                .FirstOrDefaultAsync(w => w.WalletId == walletId && w.IsActive == true);
        }

        public async Task AddAsync(Wallet wallet)
        {
            await _context.Wallets.AddAsync(wallet);
        }

        public Task UpdateAsync(Wallet wallet)
        {
            _context.Wallets.Update(wallet);
            return Task.CompletedTask;
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
        }

        public async Task<Transaction?> GetSuccessfulByTxnReferenceAsync(string txnReference)
        {
            if (string.IsNullOrWhiteSpace(txnReference))
                return null;

            return await _context.Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.TxnReference == txnReference
                    && t.Status == TransactionStatus.Success
                    && t.IsActive == true);
        }

        public async Task<(List<Transaction> Items, int TotalCount)> GetTransactionsFilteredAsync(
            long walletId,
            TransactionType? txnType,
            DateTime? fromDate,
            DateTime? toDate,
            string? search,
            int page,
            int pageSize)
        {
            var query = _context.Transactions
                .AsNoTracking()
                .Where(t => t.WalletId == walletId && t.IsActive == true);

            if (txnType.HasValue)
                query = query.Where(t => t.TxnType == txnType.Value);

            if (fromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
            {
                var end = toDate.Value.Date.AddDays(1);
                query = query.Where(t => t.CreatedAt < end);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(t =>
                    (t.Description != null && t.Description.Contains(term))
                    || (t.TxnReference != null && t.TxnReference.Contains(term))
                    || (t.ReferenceId != null && t.ReferenceId.Contains(term)));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.TxnId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<List<Transaction>> GetSuccessfulTransactionsNewestFirstAsync(long walletId, int take)
        {
            return await _context.Transactions
                .AsNoTracking()
                .Where(t =>
                    t.WalletId == walletId
                    && t.IsActive == true
                    && t.Status == TransactionStatus.Success)
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.TxnId)
                .Take(take)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
