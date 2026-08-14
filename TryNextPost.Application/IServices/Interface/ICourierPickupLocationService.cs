using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Entities;

namespace TryNextPost.Application.IServices.Interface
{
    public interface ICourierPickupLocationService
    {
        Task<CourierPickupLocation> CreateAsync(long addressId,long courierId,CancellationToken cancellationToken = default);
        Task<CourierPickupLocation?> GetAsync( long addressId, long courierId, CancellationToken cancellationToken = default);

        Task<long?> GetCourierIdAsync( string courierCode, CancellationToken cancellationToken = default);

        Task<CourierPickupLocation> GetOrCreateAsync(long addressId, long courierId, string courierCode, CancellationToken cancellationToken = default);
    }
}
