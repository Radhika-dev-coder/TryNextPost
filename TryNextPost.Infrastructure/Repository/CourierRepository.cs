using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class CourierRepository : ICourierRepository
    {
        private readonly AppDbContext _context;

        public CourierRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Courier>> GetActiveCouriersAsync()
        {
            return await _context.Couriers
                .Where(c => c.IsActive == true && !string.IsNullOrWhiteSpace(c.CourierCode))
                .OrderBy(c => c.CourierName)
                .ToListAsync();
        }

        public async Task<List<Courier>> GetAllCouriersAsync()
        {
            return await _context.Couriers
                .Where(c => !string.IsNullOrWhiteSpace(c.CourierCode))
                .OrderBy(c => c.CourierName)
                .ToListAsync();
        }

        public async Task<Courier?> GetByIdAsync(long courierId)
        {
            return await _context.Couriers
                .FirstOrDefaultAsync(c => c.CourierId == courierId && c.IsActive == true);
        }

        public async Task<Courier?> GetByIdIncludingInactiveAsync(long courierId)
        {
            return await _context.Couriers
                .FirstOrDefaultAsync(c => c.CourierId == courierId);
        }

        public async Task<Courier?> GetByCodeAsync(string courierCode)
        {
            if (string.IsNullOrWhiteSpace(courierCode))
                return null;

            var code = courierCode.Trim();
            return await _context.Couriers
                .FirstOrDefaultAsync(c =>
                    c.CourierCode == code
                    && c.IsActive == true);
        }

        public async Task UpdateAsync(Courier courier)
        {
            _context.Couriers.Update(courier);
            await _context.SaveChangesAsync();
        }
        public async Task<long?> GetCourierIdByCodeAsync(
    string courierCode,
    CancellationToken cancellationToken = default)
        {
            
            return await _context.Couriers
                .Where(x => x.CourierCode == courierCode)
                .Select(x => (long?)x.CourierId)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
