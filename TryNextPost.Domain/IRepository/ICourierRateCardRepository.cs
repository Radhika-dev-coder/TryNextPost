using TryNextPost.Domain.Entities;

namespace TryNextPost.Domain.IRepository
{
    public interface ICourierRateCardRepository
    {
        Task<CourierRateCard?> FindRateAsync(
            long courierId,
            int fromZoneId,
            int toZoneId,
            decimal weightGrams,
            string? serviceCode = null);

        Task<List<CourierRateCard>> GetByCourierAsync(long courierId);
    }
}
