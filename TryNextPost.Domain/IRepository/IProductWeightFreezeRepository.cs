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

        /// <summary>
        /// Active Requested/Accepted freeze for the same seller + product id (case-insensitive).
        /// </summary>
        Task<bool> HasActiveDuplicateAsync(long sellerId, string productId, long? excludeId = null);

        /// <summary>
        /// Accepted + AutoApply freezes for a seller that may match the given product keys (PID/SKU).
        /// </summary>
        Task<List<ProductWeightFreeze>> GetApplicableAcceptedAsync(long sellerId, IEnumerable<string> productKeys);

        Task AddAsync(ProductWeightFreeze entity);
        Task AddRangeAsync(IEnumerable<ProductWeightFreeze> entities);
        Task UpdateAsync(ProductWeightFreeze entity);
        Task SaveChangesAsync();
    }
}
