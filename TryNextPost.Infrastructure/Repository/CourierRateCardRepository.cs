using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class CourierRateCardRepository : ICourierRateCardRepository
    {
        private readonly AppDbContext _context;

        public CourierRateCardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CourierRateCard?> FindRateAsync(
            long courierId,
            int fromZoneId,
            int toZoneId,
            decimal weightGrams,
            string? serviceCode = null)
        {
            var query = _context.CourierRateCards
                .Where(r =>
                    r.CourierId == courierId
                    && r.FromZoneId == fromZoneId
                    && r.ToZoneId == toZoneId
                    && r.WeightFromGrams <= weightGrams
                    && r.WeightToGrams >= weightGrams
                    && r.IsActive == true);

            if (!string.IsNullOrWhiteSpace(serviceCode))
            {
                var code = serviceCode.Trim().ToUpperInvariant();
                query = query.Where(r => r.ServiceCode == code);
            }

            return await query
                .OrderBy(r => r.SellerCharge)
                .FirstOrDefaultAsync();
        }

        public async Task<List<CourierRateCard>> GetByCourierAsync(long courierId)
        {
            return await _context.CourierRateCards
                .Include(r => r.FromZone)
                .Include(r => r.ToZone)
                .Where(r => r.CourierId == courierId && r.IsActive == true)
                .OrderBy(r => r.FromZoneId)
                .ThenBy(r => r.ToZoneId)
                .ThenBy(r => r.WeightFromGrams)
                .ToListAsync();
        }
    }
}
