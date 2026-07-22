using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.IRepository
{
    public interface IProductWeightFreezeRepository
    {
        Task<ProductWeightFreeze?> GetByIdAsync(long id);

        Task<List<ProductWeightFreeze>> GetFilteredAsync(
            long? sellerId,
            WeightFreezeStatus? statusFilter,
            int page,
            int pageSize,
            string? productSearch,
            string? productId);

        Task<int> GetFilteredCountAsync(
            long? sellerId,
            WeightFreezeStatus? statusFilter,
            string? productSearch,
            string? productId);

        Task<int> GetCountByStatusAsync(long? sellerId, WeightFreezeStatus? status);

        Task AddAsync(ProductWeightFreeze entity);
        Task AddRangeAsync(IEnumerable<ProductWeightFreeze> entities);
        Task UpdateAsync(ProductWeightFreeze entity);
        Task SaveChangesAsync();
    }
}
