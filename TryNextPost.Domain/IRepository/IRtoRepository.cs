using TryNextPost.Domain.Entities;

namespace TryNextPost.Domain.IRepository
{
    public interface IRtoRepository
    {
        Task AddAsync(RTO rto);
        Task<RTO?> GetOpenByShipmentIdAsync(long shipmentId);
    }
}
