using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Billing;

namespace TryNextPost.Application.IServices.Interface.IBilling
{
    public interface ICreditNoteService
    {
        Task<CreditNoteListResponse> GetForSellerAsync(string userId, CreditNoteFilterRequest filter);
        Task<(byte[] Content, string FileName)> DownloadCsvForSellerAsync(string userId, long creditNoteId);
        Task<CreditNoteListResponse> GetForAdminAsync(CreditNoteFilterRequest filter);
        Task<CreditNoteListItemResponse> CreateForAdminAsync(
            string adminUserId,
            CreditNoteCreateRequest request);

        Task<InvoiceListResponse> GetInvoicesForAdminAsync(long sellerId, InvoiceFilterRequest filter);
    }
}
