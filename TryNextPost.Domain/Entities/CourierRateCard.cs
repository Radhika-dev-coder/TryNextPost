using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class CourierRateCard : BaseDbModel
    {
        [Key]
        public long CourierRateCardId { get; set; }

        public long CourierId { get; set; }
        public Courier? Courier { get; set; }

        public int FromZoneId { get; set; }
        public Zone? FromZone { get; set; }

        public int ToZoneId { get; set; }
        public Zone? ToZone { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal WeightFromGrams { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal WeightToGrams { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CourierCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SellerCharge { get; set; }

        [MaxLength(50)]
        public string ServiceCode { get; set; } = "SURFACE";

        public int EstimatedDays { get; set; } = 4;
    }
}
