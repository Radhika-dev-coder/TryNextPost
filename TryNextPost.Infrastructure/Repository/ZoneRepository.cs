using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class ZoneRepository : IZoneRepository
    {
        private readonly AppDbContext _context;

        public ZoneRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Zone>> GetAllZonesAsync()
        {
            return await _context.Zones
                .Where(z => z.IsActive == true)
                .OrderBy(z => z.ZoneCode)
                .ToListAsync();
        }

        public async Task<Zone?> GetZoneByPincodeAsync(string pincode)
        {
            var prefix = GetPincodePrefix(pincode);
            if (prefix == null)
                return null;

            var mapping = await _context.PincodeZoneMappings
                .Include(m => m.Zone)
                .FirstOrDefaultAsync(m =>
                    m.PincodePrefix == prefix
                    && m.IsActive == true
                    && m.Zone != null
                    && m.Zone.IsActive == true);

            return mapping?.Zone;
        }

        private static string? GetPincodePrefix(string pincode)
        {
            if (string.IsNullOrWhiteSpace(pincode))
                return null;

            var digits = new string(pincode.Trim().Where(char.IsDigit).ToArray());
            if (digits.Length < 2)
                return null;

            return digits[..2];
        }
    }
}
