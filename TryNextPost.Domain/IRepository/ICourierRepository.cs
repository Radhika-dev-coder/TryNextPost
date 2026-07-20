using TryNextPost.Domain.Entities;

namespace TryNextPost.Domain.IRepository
{
    public interface ICourierRepository
    {
        Task<List<Courier>> GetActiveCouriersAsync();
        Task<Courier?> GetByIdAsync(long courierId);
        Task<Courier?> GetByCodeAsync(string courierCode);
    }
}
