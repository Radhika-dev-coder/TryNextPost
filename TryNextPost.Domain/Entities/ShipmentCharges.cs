using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class ShipmentCharges : BaseDbModel
    {
        [Key]
        public long ShipmentChargesId { get; set; }

        public long ShipmentId { get; set; }
        public Shipment? Shipment { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SellerCharge { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CourierCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Margin { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CodCharge { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ChargeableWeightGrams { get; set; }

        [MaxLength(10)]
        public string? OriginZoneCode { get; set; }

        [MaxLength(10)]
        public string? DestinationZoneCode { get; set; }

        [MaxLength(50)]
        public string? ServiceCode { get; set; }
    }
}
