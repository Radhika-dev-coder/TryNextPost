using TryNextPost.Domain.Entities;

namespace TryNextPost.Domain.IRepository
{
    public interface IInvoiceRepository
    {
        Task<List<Invoice>> GetBySellerAndPeriodAsync(long sellerId, DateTime periodFrom, DateTime periodTo);
        Task<Invoice?> GetByIdAsync(long invoiceId);
        Task AddAsync(Invoice invoice);
        Task AddRangeAsync(IEnumerable<Invoice> invoices);

        Task<(List<Invoice> Items, int TotalCount)> GetFilteredAsync(
            long sellerId,
            DateTime? fromDate,
            DateTime? toDate,
            int page,
            int pageSize);

        Task SaveChangesAsync();
    }
}
