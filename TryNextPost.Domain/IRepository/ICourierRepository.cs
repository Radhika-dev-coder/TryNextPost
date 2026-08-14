using TryNextPost.Domain.Entities;

namespace TryNextPost.Domain.IRepository
{
    public interface ICourierRepository
    {
        Task<List<Courier>> GetActiveCouriersAsync();
        Task<List<Courier>> GetAllCouriersAsync();
        Task<Courier?> GetByIdAsync(long courierId);
        Task<Courier?> GetByIdIncludingInactiveAsync(long courierId);
        Task<Courier?> GetByCodeAsync(string courierCode);
        Task UpdateAsync(Courier courier);
        Task<long?> GetCourierIdByCodeAsync(string courierCode,CancellationToken cancellationToken = default);
    }
}
