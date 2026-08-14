using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class
{
    public class CourierPickupLocationService : ICourierPickupLocationService
    {
        private readonly ICourierPickupLocationRepository _repository;
        private readonly ICourierRepository _courierRepository;
        public CourierPickupLocationService(
            ICourierPickupLocationRepository repository, ICourierRepository courierRepository)
        {
            _repository = repository;
            _courierRepository = courierRepository;
        }
        public async Task<CourierPickupLocation> CreateAsync(
            long addressId,
            long courierId,
            CancellationToken cancellationToken = default)
        {
            var exists = await _repository.ExistsAsync(
                addressId,
                courierId,
                cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException(
                    "Pickup location already exists for this courier.");
            }

            var courier = await _repository.GetCourierAsync(
                courierId,
                cancellationToken);

            if (courier == null)
            {
                throw new KeyNotFoundException(
                    "Courier not found.");
            }

            var locationCode =
                $"TNX-{courier.CourierCode}-{Guid.NewGuid():N}"
                .ToUpperInvariant();

            var location = new CourierPickupLocation
            {
                AddressId = addressId,
                CourierId = courierId,
                LocationCode = locationCode
            };

            await _repository.AddAsync(
                location,
                cancellationToken);

            await _repository.SaveChangesAsync(
                cancellationToken);

            return location;
        }

        public async Task<CourierPickupLocation?> GetAsync(
    long addressId,
    long courierId,
    CancellationToken cancellationToken = default)
        {
            return await _repository.GetAsync(
                addressId,
                courierId,
                cancellationToken);
        }

        public async Task<long?> GetCourierIdAsync(
    string courierCode,
    CancellationToken cancellationToken = default)
        {
            return await _courierRepository.GetCourierIdByCodeAsync(
                courierCode,
                cancellationToken);
        }

        public async Task<CourierPickupLocation> GetOrCreateAsync(long addressId, long courierId,string courierCode, CancellationToken cancellationToken = default)
        {
            //var courierCode = await _repository.GetCourierCodeByIdAsync(courierId, cancellationToken);

            //if (string.IsNullOrWhiteSpace(courierCode))
            //{
            //    throw new InvalidOperationException(
            //        $"Courier not found for CourierId {courierId}.");
            //}

            // 1. Check existing mapping
            var existing = await _repository.GetAsync(addressId, courierId, cancellationToken);

            if (existing != null)
            {
                return existing;
            }

            // 2. Generate courier prefix
            var prefix = courierCode.Trim().ToUpperInvariant();

            if (prefix.Length < 3)
            {
                prefix = prefix.PadRight(3, 'X');
            }
            else
            {
                prefix = prefix[..3];
            }

            // 3. Get next sequence
            var nextSequence = await _repository.GetNextSequenceAsync(
                courierId,
                cancellationToken);

            // 4. Generate 13-character LocationCode
            var locationCode =
                $"{prefix}{nextSequence:D10}";

            // 5. Create mapping
            var entity = new CourierPickupLocation
            {

                AddressId = addressId,
                CourierId = courierId,
                LocationCode = locationCode,
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            };

            // 6. Save
            await _repository.AddAsync(
                entity,
                cancellationToken);

            return entity;
        }
    }
}
