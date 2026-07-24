using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.IRepository
{
    public interface ICODSettlementRepository
    {
        Task AddAsync(CODSettlement settlement);
        Task AddRangeAsync(IEnumerable<CODSettlement> settlements);
        Task UpdateAsync(CODSettlement settlement);
        Task<CODSettlement?> GetByIdAsync(long id);
        Task<CODSettlement?> GetByShipmentIdAsync(long shipmentId);
        Task<HashSet<long>> GetExistingShipmentIdsAsync(long sellerId, IEnumerable<long> shipmentIds);

        Task<(List<CODSettlement> Items, int TotalCount)> GetFilteredAsync(
            long sellerId,
            SettlementStatus? status,
            DateTime? fromDate,
            DateTime? toDate,
            int page,
            int pageSize);

        Task<decimal> SumByStatusAsync(long sellerId, SettlementStatus status);
        Task<decimal> GetLastSettledAmountAsync(long sellerId);

        Task<List<(long ShipmentId, decimal CodAmount)>> GetUnsettledDeliveredCodShipmentsAsync(long sellerId);

        Task SaveChangesAsync();
    }
}
