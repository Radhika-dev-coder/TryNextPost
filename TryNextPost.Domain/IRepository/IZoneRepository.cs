using TryNextPost.Domain.Entities;

namespace TryNextPost.Domain.IRepository
{
    public interface IZoneRepository
    {
        Task<Zone?> GetZoneByPincodeAsync(long courierId, string pincode);
        Task<List<Zone>> GetAllZonesAsync();
    }

}
