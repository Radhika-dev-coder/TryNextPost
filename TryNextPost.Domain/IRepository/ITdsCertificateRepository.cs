using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.IRepository
{
    public interface ITdsCertificateRepository
    {
        Task<TdsCertificate?> GetByIdAsync(long tdsCertificateId, bool includeSeller = false);

        Task<(List<TdsCertificate> Items, int TotalCount)> GetFilteredAsync(
            long? sellerId,
            string? financialYear,
            string? quarter,
            string? certificateSearch,
            TdsCertificateStatus? status,
            int page,
            int pageSize);

        Task AddAsync(TdsCertificate entity);
        Task UpdateAsync(TdsCertificate entity);
        Task SaveChangesAsync();
    }
}
