using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class CourierPickupLocationRepository : ICourierPickupLocationRepository
    {
        private readonly AppDbContext _appDbContext;
        public CourierPickupLocationRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task AddAsync(
            CourierPickupLocation entity,
            CancellationToken cancellationToken = default)
        {
            await _appDbContext.CourierPickupLocations
                .AddAsync(entity, cancellationToken);

            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(
            long addressId,
            long courierId,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.CourierPickupLocations
                .AnyAsync(
                    x => x.AddressId == addressId &&
                         x.CourierId == courierId,
                    cancellationToken);
        }

        public async Task<CourierPickupLocation?> GetAsync(long addressId, long courierId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.CourierPickupLocations
              .FirstOrDefaultAsync( x => x.AddressId == addressId && x.CourierId == courierId,cancellationToken);
        }

        public async Task<Courier?> GetCourierAsync(
            long courierId,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Couriers
                .FirstOrDefaultAsync(
                    x => x.CourierId == courierId,
                    cancellationToken);
        }

        public async Task<string?> GetCourierCodeByIdAsync(long courierId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Couriers
                .Where(x => x.CourierId == courierId && x.IsActive == true)
                .Select(x => x.CourierCode)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<int> GetNextSequenceAsync(long courierId, CancellationToken cancellationToken = default)
        {
            var lastLocationCode = await _appDbContext.CourierPickupLocations
                   .Where(x => x.CourierId == courierId)
                   .OrderByDescending(x => x.LocationCode)
                   .Select(x => x.LocationCode)
                   .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(lastLocationCode))
            {
                return 1;
            }

            var sequencePart = lastLocationCode.Substring(3);

            if (!int.TryParse(sequencePart, out var lastNumber))
            {
                throw new InvalidOperationException(
                    $"Invalid LocationCode format: {lastLocationCode}");
            }

            return lastNumber + 1;
        }

        public async Task<bool> LocationCodeExistsAsync(
            string locationCode,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.CourierPickupLocations
                .AnyAsync(
                    x => x.LocationCode == locationCode,
                    cancellationToken);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
