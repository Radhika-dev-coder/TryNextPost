using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class WeightDiscrepancyRepository : IWeightDiscrepancyRepository
    {
        private readonly AppDbContext _context;

        public WeightDiscrepancyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<WeightDiscrepancy?> GetByIdAsync(long id)
        {
            return await _context.WeightDiscrepancies
                .Include(w => w.Order)
                .Include(w => w.Shipment)
                .Include(w => w.Seller!)
                    .ThenInclude(s => s.Company)
                .FirstOrDefaultAsync(w => w.WeightDiscrepancyId == id && w.IsActive == true);
        }

        public async Task<List<WeightDiscrepancy>> GetFilteredAsync(
            long? sellerId,
            WeightDiscrepancyStatus? statusFilter,
            int page,
            int pageSize,
            DateTime? fromDate,
            DateTime? toDate,
            string? awbNumbers,
            string? productName,
            long? courierId)
        {
            var query = BuildQuery(sellerId, statusFilter, fromDate, toDate, awbNumbers, productName, courierId);

            return await query
                .OrderByDescending(w => w.WeightAppliedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetFilteredCountAsync(
            long? sellerId,
            WeightDiscrepancyStatus? statusFilter,
            DateTime? fromDate,
            DateTime? toDate,
            string? awbNumbers,
            string? productName,
            long? courierId)
        {
            return await BuildQuery(sellerId, statusFilter, fromDate, toDate, awbNumbers, productName, courierId)
                .CountAsync();
        }

        public async Task<int> GetCountByStatusAsync(long? sellerId, WeightDiscrepancyStatus? status)
        {
            var query = _context.WeightDiscrepancies
                .AsNoTracking()
                .Where(w => w.IsActive == true);

            if (sellerId.HasValue)
                query = query.Where(w => w.SellerId == sellerId.Value);

            if (status.HasValue)
                query = query.Where(w => w.Status == status.Value);

            return await query.CountAsync();
        }

        public async Task AddAsync(WeightDiscrepancy entity)
        {
            await _context.WeightDiscrepancies.AddAsync(entity);
        }

        public Task UpdateAsync(WeightDiscrepancy entity)
        {
            _context.WeightDiscrepancies.Update(entity);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        private IQueryable<WeightDiscrepancy> BuildQuery(
            long? sellerId,
            WeightDiscrepancyStatus? statusFilter,
            DateTime? fromDate,
            DateTime? toDate,
            string? awbNumbers,
            string? productName,
            long? courierId)
        {
            var query = _context.WeightDiscrepancies
                .AsNoTracking()
                .Include(w => w.Order)
                .Include(w => w.Seller!)
                    .ThenInclude(s => s.Company)
                .Where(w => w.IsActive == true);

            if (sellerId.HasValue)
                query = query.Where(w => w.SellerId == sellerId.Value);

            if (statusFilter.HasValue)
                query = query.Where(w => w.Status == statusFilter.Value);

            if (fromDate.HasValue)
            {
                var from = fromDate.Value.Date;
                query = query.Where(w => w.WeightAppliedDate >= from);
            }

            if (toDate.HasValue)
            {
                var toExclusive = toDate.Value.Date.AddDays(1);
                query = query.Where(w => w.WeightAppliedDate < toExclusive);
            }

            if (courierId.HasValue)
                query = query.Where(w => w.CourierId == courierId.Value);

            if (!string.IsNullOrWhiteSpace(productName))
            {
                var term = productName.Trim();
                query = query.Where(w =>
                    w.ProductName != null && w.ProductName.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(awbNumbers))
            {
                var awbs = awbNumbers
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(a => !string.IsNullOrWhiteSpace(a))
                    .ToList();

                if (awbs.Count == 1)
                {
                    var awb = awbs[0];
                    query = query.Where(w => w.AwbNumber != null && w.AwbNumber.Contains(awb));
                }
                else if (awbs.Count > 1)
                {
                    query = query.Where(w => w.AwbNumber != null && awbs.Contains(w.AwbNumber));
                }
            }

            return query;
        }
    }
}
