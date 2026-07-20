using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class ShipmentRepository : IShipmentRepository
    {
        private readonly AppDbContext _context;

        public ShipmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Shipment shipment)
        {
            await _context.Shipments.AddAsync(shipment);
        }

        public async Task<Shipment?> GetByIdAsync(long shipmentId)
        {
            return await _context.Shipments
                .Include(s => s.Courier)
                .Include(s => s.Order)
                .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId && s.IsActive == true);
        }

        public async Task<Shipment?> GetByOrderIdAsync(long orderId)
        {
            return await _context.Shipments
                .Include(s => s.Courier)
                .Where(s => s.OrderId == orderId && s.IsActive == true)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<Shipment?> GetByAwbAsync(string awbNumber)
        {
            return await _context.Shipments
                .Include(s => s.Courier)
                .Include(s => s.Order)
                .FirstOrDefaultAsync(s =>
                    s.AwbNumber == awbNumber
                    && s.IsActive == true);
        }

        public async Task AddTrackingAsync(ShipmentTracking tracking)
        {
            await _context.ShipmentTrackings.AddAsync(tracking);
        }

        public async Task<List<ShipmentTracking>> GetTrackingHistoryAsync(long shipmentId)
        {
            return await _context.ShipmentTrackings
                .AsNoTracking()
                .Where(t => t.ShipmentId == shipmentId)
                .OrderByDescending(t => t.EventTime)
                .ToListAsync();
        }

        public async Task<bool> HasActiveShipmentAsync(long orderId)
        {
            return await _context.Shipments
                .AnyAsync(s =>
                    s.OrderId == orderId
                    && s.IsActive == true
                    && s.Status != ShipmentStatus.Cancelled
                    && s.Status != ShipmentStatus.BookingFailed);
        }

        public async Task<IReadOnlyDictionary<long, Shipment>> GetActiveShipmentsByOrderIdsAsync(
            IEnumerable<long> orderIds)
        {
            var ids = orderIds.Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<long, Shipment>();

            var shipments = await _context.Shipments
                .AsNoTracking()
                .Include(s => s.Courier)
                .Where(s =>
                    ids.Contains(s.OrderId)
                    && s.IsActive == true
                    && s.Status != ShipmentStatus.Cancelled
                    && s.Status != ShipmentStatus.BookingFailed)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return shipments
                .GroupBy(s => s.OrderId)
                .ToDictionary(g => g.Key, g => g.First());
        }

        public async Task<List<Shipment>> GetBySellerFilteredAsync(
            long sellerId,
            ShipmentStatus? statusFilter,
            int page,
            int pageSize,
            string? searchQuery)
        {
            var query = BuildSellerQuery(sellerId, statusFilter, searchQuery);

            return await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetBySellerFilteredCountAsync(
            long sellerId,
            ShipmentStatus? statusFilter,
            string? searchQuery)
        {
            return await BuildSellerQuery(sellerId, statusFilter, searchQuery).CountAsync();
        }

        public async Task<int> GetCountBySellerAndStatusAsync(long sellerId, ShipmentStatus? statusFilter)
        {
            return await BuildSellerQuery(sellerId, statusFilter, null).CountAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public Task UpdateAsync(Shipment shipment)
        {
            _context.Shipments.Update(shipment);
            return Task.CompletedTask;
        }

        private IQueryable<Shipment> BuildSellerQuery(
            long sellerId,
            ShipmentStatus? statusFilter,
            string? searchQuery)
        {
            var query = _context.Shipments
                .AsNoTracking()
                .Include(s => s.Courier)
                .Include(s => s.Order)
                .Where(s => s.IsActive == true && s.Order != null && s.Order.SellerId == sellerId);

            if (statusFilter.HasValue)
                query = query.Where(s => s.Status == statusFilter.Value);

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var term = searchQuery.Trim();
                query = query.Where(s =>
                    (s.AwbNumber != null && s.AwbNumber.Contains(term))
                    || (s.Order != null && s.Order.OrderRef.Contains(term))
                    || s.DeliveryCustomerName.Contains(term)
                    || s.DeliveryPincode.Contains(term));
            }

            return query;
        }
    }
}
