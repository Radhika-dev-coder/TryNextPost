using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class SellerBankAccountRepository : ISellerBankAccountRepository
    {
        private readonly AppDbContext _context;

        public SellerBankAccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SellerBankAccount>> GetBySellerIdAsync(long sellerId)
        {
            return await _context.SellerBankAccounts
                .AsNoTracking()
                .Where(a => a.SellerId == sellerId && a.IsActive == true)
                .OrderByDescending(a => a.IsPrimary)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<SellerBankAccount?> GetByIdAsync(long id)
        {
            return await _context.SellerBankAccounts
                .FirstOrDefaultAsync(a => a.SellerBankAccountId == id && a.IsActive == true);
        }

        public async Task AddAsync(SellerBankAccount account)
        {
            await _context.SellerBankAccounts.AddAsync(account);
        }

        public Task UpdateAsync(SellerBankAccount account)
        {
            _context.SellerBankAccounts.Update(account);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
