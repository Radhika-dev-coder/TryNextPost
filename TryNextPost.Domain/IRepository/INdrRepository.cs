using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.IRepository
{
    public interface INdrRepository
    {
        Task<NDR?> GetByIdAsync(long ndrId);
        Task<NDR?> GetOpenByShipmentIdAsync(long shipmentId);
        Task<List<NDR>> GetBySellerFilteredAsync(
            long sellerId,
            NdrStatus? statusFilter,
            int page,
            int pageSize,
            string? searchQuery,
            DateTime? fromDate = null,
            DateTime? toDate = null);
        Task<int> GetBySellerFilteredCountAsync(
            long sellerId,
            NdrStatus? statusFilter,
            string? searchQuery,
            DateTime? fromDate = null,
            DateTime? toDate = null);
        Task<int> GetCountBySellerAndStatusAsync(long sellerId, NdrStatus? status);
        Task AddAsync(NDR ndr);
        Task UpdateAsync(NDR ndr);
        Task SaveChangesAsync();
    }
}
