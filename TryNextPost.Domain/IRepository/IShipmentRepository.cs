using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.IRepository
{
    public interface IShipmentRepository
    {
        Task AddAsync(Shipment shipment);
        Task<Shipment?> GetByIdAsync(long shipmentId);
        Task<Shipment?> GetByOrderIdAsync(long orderId);
        Task<Shipment?> GetByAwbAsync(string awbNumber);
        Task<bool> HasActiveShipmentAsync(long orderId);

        /// <summary>
        /// Latest active (non-cancelled / non-booking-failed) shipment per order id.
        /// </summary>
        Task<IReadOnlyDictionary<long, Shipment>> GetActiveShipmentsByOrderIdsAsync(
            IEnumerable<long> orderIds);

        Task AddTrackingAsync(ShipmentTracking tracking);
        Task<List<ShipmentTracking>> GetTrackingHistoryAsync(long shipmentId);
        Task<List<Shipment>> GetBySellerFilteredAsync(
            long sellerId,
            ShipmentStatus? statusFilter,
            int page,
            int pageSize,
            string? searchQuery);
        Task<int> GetBySellerFilteredCountAsync(
            long sellerId,
            ShipmentStatus? statusFilter,
            string? searchQuery);
        Task<int> GetCountBySellerAndStatusAsync(long sellerId, ShipmentStatus? statusFilter);
        Task SaveChangesAsync();
        Task UpdateAsync(Shipment shipment);
    }
}
