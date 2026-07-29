using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.IRepository
{
    public interface ICreditNoteRepository
    {
        Task<CreditNote> GetByIdAsync(long CreditNoteId, bool includeInvoice = false);

        Task<(List<CreditNote> Items, int Totalount)> GetFilteredAsync(long? sellerId,
            DateTime? fromDate,
            DateTime? toDate,
            CreditNoteStatus? status,
            int page, int pageSize);

        Task<int> CountForSellerInMonthAsync(long SellerId, int year, int month);
        Task AddAsync(CreditNote entity);
        Task UpdateAsync(CreditNote entity);
        Task SaveChangesAsync();
    }
}
