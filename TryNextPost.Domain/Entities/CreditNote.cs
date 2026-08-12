using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities
{
    public class CreditNote : BaseDbModel
    {
        [Key]
        public long CreditNoteId { get; set; }
        public long SellerId { get; set; }
        public Seller? Seller { get; set; }
        public long? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        [MaxLength(50)]
        public string CreditNoteNumber { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public CreditNoteReasonType ReasonType { get; set; }
        public CreditNoteStatus Status { get; set; } = CreditNoteStatus.Issued;
        [MaxLength(500)]
        public string? Remark { get; set; }
        [MaxLength(100)]
        public string Period { get; set; } = string.Empty;
        public DateTime CreditNoteDate { get; set; }
        [MaxLength(500)]
        public string? FilePath { get; set; }
    }
}
