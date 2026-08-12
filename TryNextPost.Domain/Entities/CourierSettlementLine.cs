using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class CourierSettlementLine : BaseDbModel
    {
        [Key]
        public long CourierSettlementLineId { get; set; }

        public long CourierSettlementId { get; set; }
        public CourierSettlement? CourierSettlement { get; set; }

        public long ShipmentId { get; set; }
        public Shipment? Shipment { get; set; }

        [MaxLength(100)]
        public string? AwbNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CourierCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SellerCharge { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Margin { get; set; }

        public DateTime? ShipmentBookedAt { get; set; }
    }
}
