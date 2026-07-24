using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class Invoice : BaseDbModel
    {
        [Key]
        public long InvoiceId { get; set; }

        public long SellerId { get; set; }
        public Seller? Seller { get; set; }

        [MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ServiceType { get; set; } = "Combined";

        public DateTime InvoiceDate { get; set; }

        public DateTime PeriodFrom { get; set; }

        public DateTime PeriodTo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingChargesAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RechargeAmount { get; set; }
    }
}
