using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities
{
    public class CourierSettlement : BaseDbModel
    {
        [Key]
        public long CourierSettlementId { get; set; }

        public long CourierId { get; set; }
        public Courier? Courier { get; set; }

        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCourierCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSellerCharge { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalMargin { get; set; }

        public int ShipmentCount { get; set; }

        public SettlementStatus Status { get; set; } = SettlementStatus.Pending;

        public DateTime? SettledAt { get; set; }

        [MaxLength(200)]
        public string? PaymentReference { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public ICollection<CourierSettlementLine>? Lines { get; set; }
    }
}
