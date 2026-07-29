using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Enums
{
    public enum CreditNoteReasonType
    {
        InvoiceCorrection = 1,
        RemittanceAdjustment = 2,
        WeightDispute = 3,
        Other = 4
    }
}
