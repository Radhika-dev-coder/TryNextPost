using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class NdrRepository : INdrRepository
    {
        private readonly AppDbContext _context;

        public NdrRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<NDR?> GetByIdAsync(long ndrId)
        {
            return await _context.NDRS
                .Include(n => n.Shipment)!
                    .ThenInclude(s => s!.Courier)
                .Include(n => n.Shipment)!
                    .ThenInclude(s => s!.Order)!
                        .ThenInclude(o => o!.OrderItems)
                .FirstOrDefaultAsync(n => n.NdrId == ndrId && n.IsActive == true);
        }

        public async Task<NDR?> GetOpenByShipmentIdAsync(long shipmentId)
        {
            return await _context.NDRS
                .Where(n =>
                    n.ShipmentId == shipmentId
                    && n.IsActive == true
                    && (n.Status == NdrStatus.ActionRequired || n.Status == NdrStatus.ActionRequested))
                .OrderByDescending(n => n.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<NDR>> GetBySellerFilteredAsync(
            long sellerId,
            NdrStatus? statusFilter,
            int page,
            int pageSize,
            string? searchQuery,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = BuildSellerQuery(sellerId, statusFilter, searchQuery, fromDate, toDate);

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetBySellerFilteredCountAsync(
            long sellerId,
            NdrStatus? statusFilter,
            string? searchQuery,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            return await BuildSellerQuery(sellerId, statusFilter, searchQuery, fromDate, toDate).CountAsync();
        }

        public async Task<int> GetCountBySellerAndStatusAsync(long sellerId, NdrStatus? status)
        {
            var query = _context.NDRS
                .AsNoTracking()
                .Where(n =>
                    n.IsActive == true
                    && n.Shipment != null
                    && n.Shipment.Order != null
                    && n.Shipment.Order.SellerId == sellerId);

            if (status.HasValue)
                query = query.Where(n => n.Status == status.Value);

            return await query.CountAsync();
        }

        public async Task AddAsync(NDR ndr)
        {
            await _context.NDRS.AddAsync(ndr);
        }

        public Task UpdateAsync(NDR ndr)
        {
            _context.NDRS.Update(ndr);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        private IQueryable<NDR> BuildSellerQuery(
            long sellerId,
            NdrStatus? statusFilter,
            string? searchQuery,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.NDRS
                .AsNoTracking()
                .Include(n => n.Shipment)!
                    .ThenInclude(s => s!.Courier)
                .Include(n => n.Shipment)!
                    .ThenInclude(s => s!.Order)!
                        .ThenInclude(o => o!.OrderItems)
                .Where(n =>
                    n.IsActive == true
                    && n.Shipment != null
                    && n.Shipment.Order != null
                    && n.Shipment.Order.SellerId == sellerId);

            if (statusFilter.HasValue)
                query = query.Where(n => n.Status == statusFilter.Value);

            if (fromDate.HasValue)
            {
                var from = fromDate.Value.Date;
                query = query.Where(n => n.CreatedAt != null && n.CreatedAt >= from);
            }

            if (toDate.HasValue)
            {
                var toExclusive = toDate.Value.Date.AddDays(1);
                query = query.Where(n => n.CreatedAt != null && n.CreatedAt < toExclusive);
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var term = searchQuery.Trim();
                query = query.Where(n =>
                    (n.Shipment!.AwbNumber != null && n.Shipment.AwbNumber.Contains(term))
                    || (n.Shipment.Order != null && n.Shipment.Order.OrderRef.Contains(term))
                    || (n.Shipment.Order != null && n.Shipment.Order.CustomerName.Contains(term))
                    || (n.Shipment.Order != null && n.Shipment.Order.CustomerMobile.Contains(term))
                    || n.Reason.Contains(term));
            }

            return query;
        }
    }
}
