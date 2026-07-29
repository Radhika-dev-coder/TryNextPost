using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class TdsCertificateRepository : ITdsCertificateRepository
    {
        private readonly AppDbContext _context;

        public TdsCertificateRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TdsCertificate?> GetByIdAsync(long tdsCertificateId, bool includeSeller = false)
        {
            var query = _context.TdsCertificates.AsQueryable();
            if (includeSeller)
            {
                query = query
                    .Include(t => t.Seller)!
                    .ThenInclude(s => s!.Company);
            }

            return await query.FirstOrDefaultAsync(t =>
                t.TdsCertificateId == tdsCertificateId && t.IsActive == true);
        }

        public async Task<(List<TdsCertificate> Items, int TotalCount)> GetFilteredAsync(
            long? sellerId,
            string? financialYear,
            string? quarter,
            string? certificateSearch,
            TdsCertificateStatus? status,
            int page,
            int pageSize)
        {
            var query = _context.TdsCertificates
                .AsNoTracking()
                .Include(t => t.Seller)!
                .ThenInclude(s => s!.Company)
                .Where(t => t.IsActive == true);

            if (sellerId.HasValue)
                query = query.Where(t => t.SellerId == sellerId.Value);

            if (!string.IsNullOrWhiteSpace(financialYear))
            {
                var fy = financialYear.Trim();
                query = query.Where(t => t.FinancialYear == fy);
            }

            if (!string.IsNullOrWhiteSpace(quarter))
            {
                var q = quarter.Trim().ToUpperInvariant();
                query = query.Where(t => t.Quarter == q);
            }

            if (!string.IsNullOrWhiteSpace(certificateSearch))
            {
                var search = certificateSearch.Trim();
                query = query.Where(t => t.CertificateNumber.Contains(search));
            }

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.PeriodFrom)
                .ThenByDescending(t => t.TdsCertificateId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task AddAsync(TdsCertificate entity)
        {
            await _context.TdsCertificates.AddAsync(entity);
        }

        public Task UpdateAsync(TdsCertificate entity)
        {
            _context.TdsCertificates.Update(entity);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
