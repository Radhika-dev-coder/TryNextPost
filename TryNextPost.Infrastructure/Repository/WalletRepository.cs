using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
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

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
