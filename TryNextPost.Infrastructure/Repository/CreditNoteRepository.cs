using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;
using TryNextPost.Infrastructure.Migrations;

namespace TryNextPost.Infrastructure.Repository
{
    public class CreditNoteRepository : ICreditNoteRepository
    {
        private readonly AppDbContext _appDbContext;
        public CreditNoteRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddAsync(CreditNote entity)
        {
             await _appDbContext.creditNotes.AddAsync(entity);
        }

        public async Task<int> CountForSellerInMonthAsync(long SellerId, int year, int month)
        {
            return await _appDbContext.creditNotes.CountAsync(c => c.SellerId == SellerId
            && c.CreditNoteDate.Year == year
            && c.CreditNoteDate.Month == month);
        }

        public async Task<CreditNote> GetByIdAsync(long CreditNoteId, bool includeInvoice = false)
        {
            var query = _appDbContext.creditNotes.AsQueryable();
            if (includeInvoice)
                query = query.Include(c => c.Invoice);
            return await query.FirstOrDefaultAsync(c =>
                c.CreditNoteId == CreditNoteId && c.IsActive == true);
        }

        public async Task<(List<CreditNote> Items, int Totalount)> GetFilteredAsync(long? sellerId, DateTime? fromDate, DateTime? toDate, CreditNoteStatus? status, int page, int pageSize)
        {
            var query = _appDbContext.creditNotes
                .AsNoTracking()
                .Include(c => c.Invoice)
                .Include(c => c.Seller)
                .Where(c => c.IsActive == true);

            if(sellerId.HasValue)
                query = query.Where(c => c.SellerId == sellerId.Value);

            if(fromDate.HasValue)
                query = query.Where(c => c.CreditNoteDate >=  fromDate.Value);

            if(toDate.HasValue)
                query = query.Where(c => c.CreditNoteDate <= toDate.Value);

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            var total = await query.CountAsync();
            var item = await query 
                .OrderByDescending(c => c.CreditNoteDate)
                .ThenByDescending(c => c.CreditNoteId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return(item,total);
        }

        public async Task SaveChangesAsync()
        {
            await _appDbContext.SaveChangesAsync();
        }

        public Task UpdateAsync(CreditNote entity)
        {
           _appDbContext.creditNotes.Update(entity);
            return Task.CompletedTask;
        }
    }
}
