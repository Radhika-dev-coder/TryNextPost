using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Entities;

namespace TryNextPost.Domain.IRepository
{
    public interface ICourierPickupLocationRepository
    {

        Task<bool> ExistsAsync(long addressId, long courierId, CancellationToken cancellationToken = default);

        Task<Courier?> GetCourierAsync(long courierId, CancellationToken cancellationToken = default);

        Task<bool> LocationCodeExistsAsync(string locationCode, CancellationToken cancellationToken = default);

        Task AddAsync( CourierPickupLocation entity, CancellationToken cancellationToken = default);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<CourierPickupLocation?> GetAsync( long addressId, long courierId, CancellationToken cancellationToken = default);

        Task<int> GetNextSequenceAsync( long courierId,CancellationToken cancellationToken = default);

        Task<string?> GetCourierCodeByIdAsync(long courierId, CancellationToken cancellationToken = default);
    }
}
