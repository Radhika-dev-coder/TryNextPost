using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

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
        public ServiceType ServiceType { get; set; }
        public bool IsCodAvailable { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CodCharge { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CodPercentage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinimumCharge { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal FuelSurchargePercent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HandlingCharge { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal RtoCharge { get; set; }

        public string PaymentType { get; set; } = "Both";
        public int Priority { get; set; }
        public int MinDays { get; set; }
        public int MaxDays { get; set; }
    }
}
