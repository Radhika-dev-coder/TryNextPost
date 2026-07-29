using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Billing
{
    public class CreditNoteListItemResponse
    {
        public long CreditNoteId { get; set; }
        public long SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public long? InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public string CreditNoteNumber { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime CreditNoteDate { get; set; }
        public string Period { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int ReasonType { get; set; }
        public string ReasonTypeName { get; set; } = string.Empty;
        public string? Remark { get; set; }
    }
}
