using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AppDbContext _context;

        public InvoiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Invoice>> GetBySellerAndPeriodAsync(
            long sellerId,
            DateTime periodFrom,
            DateTime periodTo)
        {
            return await _context.Invoices
                .AsNoTracking()
                .Where(i =>
                    i.SellerId == sellerId
                    && i.IsActive == true
                    && i.PeriodFrom == periodFrom
                    && i.PeriodTo == periodTo)
                .ToListAsync();
        }

        public async Task<Invoice?> GetByIdAsync(long invoiceId)
        {
            return await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && i.IsActive == true);
        }

        public async Task AddAsync(Invoice invoice)
        {
            await _context.Invoices.AddAsync(invoice);
        }

        public async Task AddRangeAsync(IEnumerable<Invoice> invoices)
        {
            await _context.Invoices.AddRangeAsync(invoices);
        }

        public async Task<(List<Invoice> Items, int TotalCount)> GetFilteredAsync(
            long sellerId,
            DateTime? fromDate,
            DateTime? toDate,
            int page,
            int pageSize)
        {
            var query = _context.Invoices
                .AsNoTracking()
                .Where(i => i.SellerId == sellerId && i.IsActive == true);

            if (fromDate.HasValue)
                query = query.Where(i => i.InvoiceDate >= fromDate.Value);

            if (toDate.HasValue)
            {
                var end = toDate.Value.Date.AddDays(1);
                query = query.Where(i => i.InvoiceDate < end);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(i => i.InvoiceDate)
                .ThenByDescending(i => i.InvoiceId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
