using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Billing
{
    public class CreditNoteCreateRequest
    {
        public long InvoiceId { get; set; }
        public decimal Amount { get; set; }
        /// 1=InvoiceCorrection, 2=RemittanceAdjustment, 3=WeightDispute, 4=Other
        public int ReasonType { get; set; }
        public string? Remark { get; set; }
        public bool ApplyToWallet { get; set; } = true;
    }
}
