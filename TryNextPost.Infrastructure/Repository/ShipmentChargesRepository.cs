using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class ShipmentChargesRepository : IShipmentChargesRepository
    {
        private readonly AppDbContext _context;

        public ShipmentChargesRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ShipmentCharges charges)
        {
            await _context.ShipmentCharges.AddAsync(charges);
        }

        public async Task<ShipmentCharges?> GetByShipmentIdAsync(long shipmentId)
        {
            return await _context.ShipmentCharges
                .FirstOrDefaultAsync(c => c.ShipmentId == shipmentId && c.IsActive == true);
        }

        public async Task<(List<ShipmentCharges> Items, int TotalCount)> GetFilteredForSellerAsync(
            long sellerId,
            DateTime? fromDate,
            DateTime? toDate,
            IReadOnlyList<string>? awbNumbers,
            int page,
            int pageSize)
        {
            var query = _context.ShipmentCharges
                .AsNoTracking()
                .Include(c => c.Shipment)!
                    .ThenInclude(s => s!.Courier)
                .Include(c => c.Shipment)!
                    .ThenInclude(s => s!.Order)
                .Where(c => c.IsActive == true && c.Shipment != null && c.Shipment.Order != null
                            && c.Shipment.Order.SellerId == sellerId);

            if (fromDate.HasValue)
                query = query.Where(c => c.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
            {
                var end = toDate.Value.Date.AddDays(1);
                query = query.Where(c => c.CreatedAt < end);
            }

            if (awbNumbers is { Count: > 0 })
            {
                query = query.Where(c =>
                    c.Shipment != null
                    && c.Shipment.AwbNumber != null
                    && awbNumbers.Contains(c.Shipment.AwbNumber));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .ThenByDescending(c => c.ShipmentChargesId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<decimal> SumSellerChargeForPeriodAsync(
            long sellerId,
            DateTime periodFrom,
            DateTime periodTo)
        {
            var end = periodTo.Date.AddDays(1);
            return await (
                from c in _context.ShipmentCharges.AsNoTracking()
                join s in _context.Shipments.AsNoTracking() on c.ShipmentId equals s.ShipmentId
                join o in _context.Orders.AsNoTracking() on s.OrderId equals o.OrderId
                where c.IsActive == true
                      && o.SellerId == sellerId
                      && c.CreatedAt >= periodFrom
                      && c.CreatedAt < end
                select (decimal?)c.SellerCharge
            ).SumAsync() ?? 0m;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
