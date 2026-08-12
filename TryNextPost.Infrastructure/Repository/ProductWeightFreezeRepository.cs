using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class ProductWeightFreezeRepository : IProductWeightFreezeRepository
    {
        private readonly AppDbContext _context;

        public ProductWeightFreezeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProductWeightFreeze?> GetByIdAsync(long id)
        {
            return await _context.ProductWeightFreezes
                .Include(w => w.Seller!)
                    .ThenInclude(s => s.Company)
                .FirstOrDefaultAsync(w => w.ProductWeightFreezeId == id && w.IsActive == true);
        }

        public async Task<List<ProductWeightFreeze>> GetFilteredAsync(
            long? sellerId,
            WeightFreezeStatus? statusFilter,
            int page,
            int pageSize,
            string? productSearch,
            string? productId)
        {
            var query = BuildQuery(sellerId, statusFilter, productSearch, productId);

            return await query
                .OrderByDescending(w => w.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetFilteredCountAsync(
            long? sellerId,
            WeightFreezeStatus? statusFilter,
            string? productSearch,
            string? productId)
        {
            return await BuildQuery(sellerId, statusFilter, productSearch, productId).CountAsync();
        }

        public async Task<int> GetCountByStatusAsync(long? sellerId, WeightFreezeStatus? status)
        {
            var query = _context.ProductWeightFreezes
                .AsNoTracking()
                .Where(w => w.IsActive == true);

            if (sellerId.HasValue)
                query = query.Where(w => w.SellerId == sellerId.Value);

            if (status.HasValue)
                query = query.Where(w => w.Status == status.Value);

            return await query.CountAsync();
        }

        public async Task<bool> HasActiveDuplicateAsync(long sellerId, string productId, long? excludeId = null)
        {
            var pid = productId.Trim();
            var query = _context.ProductWeightFreezes
                .AsNoTracking()
                .Where(w =>
                    w.IsActive == true
                    && w.SellerId == sellerId
                    && (w.Status == WeightFreezeStatus.Requested || w.Status == WeightFreezeStatus.Accepted)
                    && w.ProductId.ToLower() == pid.ToLower());

            if (excludeId.HasValue)
                query = query.Where(w => w.ProductWeightFreezeId != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<List<ProductWeightFreeze>> GetApplicableAcceptedAsync(
            long sellerId,
            IEnumerable<string> productKeys)
        {
            var keys = productKeys
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Select(k => k.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            if (keys.Count == 0)
                return new List<ProductWeightFreeze>();

            var candidates = await _context.ProductWeightFreezes
                .AsNoTracking()
                .Where(w =>
                    w.IsActive == true
                    && w.SellerId == sellerId
                    && w.Status == WeightFreezeStatus.Accepted
                    && w.AutoApply)
                .ToListAsync();

            return candidates
                .Where(w =>
                {
                    var pid = w.ProductId?.Trim().ToLowerInvariant();
                    var sku = w.Sku?.Trim().ToLowerInvariant();
                    return (!string.IsNullOrEmpty(pid) && keys.Contains(pid))
                           || (!string.IsNullOrEmpty(sku) && keys.Contains(sku));
                })
                .ToList();
        }

        public async Task AddAsync(ProductWeightFreeze entity)
        {
            await _context.ProductWeightFreezes.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<ProductWeightFreeze> entities)
        {
            await _context.ProductWeightFreezes.AddRangeAsync(entities);
        }

        public Task UpdateAsync(ProductWeightFreeze entity)
        {
            _context.ProductWeightFreezes.Update(entity);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        private IQueryable<ProductWeightFreeze> BuildQuery(
            long? sellerId,
            WeightFreezeStatus? statusFilter,
            string? productSearch,
            string? productId)
        {
            var query = _context.ProductWeightFreezes
                .AsNoTracking()
                .Include(w => w.Seller!)
                    .ThenInclude(s => s.Company)
                .Where(w => w.IsActive == true);

            if (sellerId.HasValue)
                query = query.Where(w => w.SellerId == sellerId.Value);

            if (statusFilter.HasValue)
                query = query.Where(w => w.Status == statusFilter.Value);

            if (!string.IsNullOrWhiteSpace(productId))
            {
                var pid = productId.Trim();
                query = query.Where(w => w.ProductId.Contains(pid));
            }

            if (!string.IsNullOrWhiteSpace(productSearch))
            {
                var term = productSearch.Trim();
                query = query.Where(w =>
                    w.ProductName.Contains(term)
                    || (w.Sku != null && w.Sku.Contains(term)));
            }

            return query;
        }
    }
}
