using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class CODSettlementRepository : ICODSettlementRepository
    {
        private readonly AppDbContext _context;

        public CODSettlementRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CODSettlement settlement)
        {
            await _context.CODSettlements.AddAsync(settlement);
        }

        public async Task AddRangeAsync(IEnumerable<CODSettlement> settlements)
        {
            await _context.CODSettlements.AddRangeAsync(settlements);
        }

        public Task UpdateAsync(CODSettlement settlement)
        {
            _context.CODSettlements.Update(settlement);
            return Task.CompletedTask;
        }

        public async Task<CODSettlement?> GetByIdAsync(long id)
        {
            return await _context.CODSettlements
                .Include(c => c.Shipment)
                .FirstOrDefaultAsync(c => c.CodSettlementId == id && c.IsActive == true);
        }

        public async Task<CODSettlement?> GetByShipmentIdAsync(long shipmentId)
        {
            return await _context.CODSettlements
                .FirstOrDefaultAsync(c => c.ShipmentId == shipmentId && c.IsActive == true);
        }

        public async Task<HashSet<long>> GetExistingShipmentIdsAsync(long sellerId, IEnumerable<long> shipmentIds)
        {
            var ids = shipmentIds.Distinct().ToList();
            if (ids.Count == 0)
                return new HashSet<long>();

            var existing = await _context.CODSettlements
                .AsNoTracking()
                .Where(c => c.SellerId == sellerId && ids.Contains(c.ShipmentId))
                .Select(c => c.ShipmentId)
                .ToListAsync();

            return existing.ToHashSet();
        }

        public async Task<(List<CODSettlement> Items, int TotalCount)> GetFilteredAsync(
            long sellerId,
            SettlementStatus? status,
            DateTime? fromDate,
            DateTime? toDate,
            int page,
            int pageSize)
        {
            var query = _context.CODSettlements
                .AsNoTracking()
                .Include(c => c.Shipment)
                .Where(c => c.SellerId == sellerId && c.IsActive == true);

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            if (fromDate.HasValue)
                query = query.Where(c => (c.SettlementDate ?? c.CreatedAt) >= fromDate.Value);

            if (toDate.HasValue)
            {
                var end = toDate.Value.Date.AddDays(1);
                query = query.Where(c => (c.SettlementDate ?? c.CreatedAt) < end);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.SettlementDate ?? c.CreatedAt)
                .ThenByDescending(c => c.CodSettlementId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<decimal> SumByStatusAsync(long sellerId, SettlementStatus status)
        {
            return await _context.CODSettlements
                .AsNoTracking()
                .Where(c => c.SellerId == sellerId && c.IsActive == true && c.Status == status)
                .SumAsync(c => (decimal?)(c.CollectedAmount > 0 ? c.CollectedAmount : c.CodAmount)) ?? 0m;
        }

        public async Task<decimal> GetLastSettledAmountAsync(long sellerId)
        {
            var last = await _context.CODSettlements
                .AsNoTracking()
                .Where(c => c.SellerId == sellerId && c.IsActive == true && c.Status == SettlementStatus.Settled)
                .OrderByDescending(c => c.SettlementDate ?? c.UpdatedAt ?? c.CreatedAt)
                .ThenByDescending(c => c.CodSettlementId)
                .FirstOrDefaultAsync();

            if (last == null)
                return 0m;

            return last.CollectedAmount > 0 ? last.CollectedAmount : last.CodAmount;
        }

        /// <summary>
        /// Delivered COD shipments for seller that do not yet have a CODSettlement row.
        /// </summary>
        public async Task<List<(long ShipmentId, decimal CodAmount)>> GetUnsettledDeliveredCodShipmentsAsync(long sellerId)
        {
            var existingIds = _context.CODSettlements
                .Where(c => c.SellerId == sellerId)
                .Select(c => c.ShipmentId);

            var rows = await _context.Shipments
                .AsNoTracking()
                .Include(s => s.Order)
                .Where(s =>
                    s.IsActive == true
                    && s.Status == ShipmentStatus.Delivered
                    && s.Order != null
                    && s.Order.SellerId == sellerId
                    && s.Order.PaymentMode == PaymentMode.COD
                    && !existingIds.Contains(s.ShipmentId))
                .Select(s => new
                {
                    s.ShipmentId,
                    Amount = s.Order!.FinalPayableAmount > 0 ? s.Order.FinalPayableAmount : s.Order.TotalAmount
                })
                .ToListAsync();

            return rows.Select(r => (r.ShipmentId, r.Amount)).ToList();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
