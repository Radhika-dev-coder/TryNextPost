using TryNextPost.Domain.Entities;

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
        Task SaveChangesAsync();
    }
}
