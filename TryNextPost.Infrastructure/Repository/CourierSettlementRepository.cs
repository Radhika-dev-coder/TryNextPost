using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class CourierSettlementRepository : ICourierSettlementRepository
    {
        private readonly AppDbContext _context;

        public CourierSettlementRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CourierSettlement settlement)
        {
            await _context.CourierSettlements.AddAsync(settlement);
        }

        public async Task<CourierSettlement?> GetByIdAsync(long settlementId)
        {
            return await _context.CourierSettlements
                .Include(s => s.Courier)
                .Include(s => s.Lines)
                .FirstOrDefaultAsync(s =>
                    s.CourierSettlementId == settlementId
                    && s.IsActive == true);
        }

        public async Task<List<CourierSettlement>> GetByCourierAndPeriodAsync(
            long? courierId,
            DateTime? from,
            DateTime? to)
        {
            var query = _context.CourierSettlements
                .Include(s => s.Courier)
                .Include(s => s.Lines)
                .Where(s => s.IsActive == true);

            if (courierId.HasValue)
                query = query.Where(s => s.CourierId == courierId.Value);

            if (from.HasValue)
                query = query.Where(s => s.PeriodTo >= from.Value);

            if (to.HasValue)
                query = query.Where(s => s.PeriodFrom <= to.Value);

            return await query
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<HashSet<long>> GetSettledShipmentIdsAsync(long courierId)
        {
            var ids = await _context.CourierSettlementLines
                .Where(l =>
                    l.IsActive == true
                    && l.CourierSettlement != null
                    && l.CourierSettlement.CourierId == courierId
                    && l.CourierSettlement.IsActive == true)
                .Select(l => l.ShipmentId)
                .ToListAsync();

            return ids.ToHashSet();
        }

        public async Task<List<ShipmentCharges>> GetUnsettledChargesAsync(
            long courierId,
            DateTime periodFrom,
            DateTime periodTo)
        {
            return await _context.ShipmentCharges
                .Include(c => c.Shipment)
                .Where(c =>
                    c.IsActive == true
                    && c.Shipment != null
                    && c.Shipment.CourierId == courierId
                    && c.Shipment.IsActive == true
                    && c.Shipment.CreatedAt >= periodFrom
                    && c.Shipment.CreatedAt <= periodTo)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CourierSettlement settlement)
        {
            _context.CourierSettlements.Update(settlement);
            await Task.CompletedTask;
        }
    }
}
