using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.IRepository
{
    public interface IShipmentChargesRepository
    {
        Task AddAsync(ShipmentCharges charges);
        Task<ShipmentCharges?> GetByShipmentIdAsync(long shipmentId);

        Task<(List<ShipmentCharges> Items, int TotalCount)> GetFilteredForSellerAsync(
            long sellerId,
            DateTime? fromDate,
            DateTime? toDate,
            IReadOnlyList<string>? awbNumbers,
            int page,
            int pageSize);

        Task<decimal> SumSellerChargeForPeriodAsync(long sellerId, DateTime periodFrom, DateTime periodTo);

        Task SaveChangesAsync();
    }
}
