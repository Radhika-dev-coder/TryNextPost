using System.ComponentModel.DataAnnotations;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities
{
    public class ProductWeightFreeze : BaseDbModel
    {
        [Key]
        public long ProductWeightFreezeId { get; set; }

        public long SellerId { get; set; }
        public Seller? Seller { get; set; }

        /// <summary>Seller-facing product identifier (PID).</summary>
        public string ProductId { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;
        public string? Sku { get; set; }

        public decimal LengthCm { get; set; }
        public decimal BreadthCm { get; set; }
        public decimal HeightCm { get; set; }
        public decimal WeightGrams { get; set; }

        public bool AutoApply { get; set; }

        public WeightFreezeStatus Status { get; set; } = WeightFreezeStatus.Requested;

        public string? ActionRemarks { get; set; }
        public DateTime? ActionedAt { get; set; }
        public string? ActionedBy { get; set; }
    }
}
