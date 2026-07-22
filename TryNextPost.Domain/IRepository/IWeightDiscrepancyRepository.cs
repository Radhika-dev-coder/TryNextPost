using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.IRepository
{
    public interface IWeightDiscrepancyRepository
    {
        Task<WeightDiscrepancy?> GetByIdAsync(long id);

        Task<List<WeightDiscrepancy>> GetFilteredAsync(
            long? sellerId,
            WeightDiscrepancyStatus? statusFilter,
            int page,
            int pageSize,
            DateTime? fromDate,
            DateTime? toDate,
            string? awbNumbers,
            string? productName,
            long? courierId);

        Task<int> GetFilteredCountAsync(
            long? sellerId,
            WeightDiscrepancyStatus? statusFilter,
            DateTime? fromDate,
            DateTime? toDate,
            string? awbNumbers,
            string? productName,
            long? courierId);

        Task<int> GetCountByStatusAsync(long? sellerId, WeightDiscrepancyStatus? status);

        Task AddAsync(WeightDiscrepancy entity);
        Task UpdateAsync(WeightDiscrepancy entity);
        Task SaveChangesAsync();
    }
}
