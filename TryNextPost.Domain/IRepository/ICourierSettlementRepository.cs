using TryNextPost.Domain.Entities;

namespace TryNextPost.Domain.IRepository
{
    public interface ICourierSettlementRepository
    {
        Task<CourierSettlement?> GetByIdAsync(long settlementId);
        Task<List<ShipmentCharges>> GetUnsettledChargesAsync(
            long courierId,
            DateTime periodFrom,
            DateTime periodTo);

        Task<HashSet<long>> GetSettledShipmentIdsAsync(long courierId);

        Task AddAsync(CourierSettlement settlement);
        Task UpdateAsync(CourierSettlement settlement);
        Task SaveChangesAsync();

        Task<List<CourierSettlement>> GetByCourierAndPeriodAsync(
            long? courierId,
            DateTime? from,
            DateTime? to);
    }
}
