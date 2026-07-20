using TryNextPost.Domain.Entities;

namespace TryNextPost.Domain.IRepository
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetByUserIdAsync(string userId);
        Task AddAsync(Wallet wallet);
        Task UpdateAsync(Wallet wallet);
        Task AddTransactionAsync(Transaction transaction);
        Task SaveChangesAsync();
    }
}
