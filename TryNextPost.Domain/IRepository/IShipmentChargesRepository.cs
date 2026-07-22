using TryNextPost.Domain.Entities;

namespace TryNextPost.Domain.IRepository
{
    public interface IShipmentChargesRepository
    {
        Task AddAsync(ShipmentCharges charges);
        Task<ShipmentCharges?> GetByShipmentIdAsync(long shipmentId);
        Task SaveChangesAsync();
    }
}
