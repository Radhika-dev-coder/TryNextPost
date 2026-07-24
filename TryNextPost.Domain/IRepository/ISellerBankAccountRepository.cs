using TryNextPost.Domain.Entities;

namespace TryNextPost.Domain.IRepository
{
    public interface ISellerBankAccountRepository
    {
        Task<List<SellerBankAccount>> GetBySellerIdAsync(long sellerId);
        Task<SellerBankAccount?> GetByIdAsync(long id);
        Task AddAsync(SellerBankAccount account);
        Task UpdateAsync(SellerBankAccount account);
        Task SaveChangesAsync();
    }
}
