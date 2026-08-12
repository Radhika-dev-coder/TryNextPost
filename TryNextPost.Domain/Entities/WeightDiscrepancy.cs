using System.ComponentModel.DataAnnotations;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities
{
    public class WeightDiscrepancy : BaseDbModel
    {
        [Key]
        public long WeightDiscrepancyId { get; set; }

        public long SellerId { get; set; }
        public Seller? Seller { get; set; }

        public long? ShipmentId { get; set; }
        public Shipment? Shipment { get; set; }

        public long? OrderId { get; set; }
        public Order? Order { get; set; }

        public string? AwbNumber { get; set; }
        public long? CourierId { get; set; }
        public string? CourierName { get; set; }
        public string? ProductName { get; set; }

        /// <summary>Seller-declared / booked weight in grams.</summary>
        public decimal EnteredWeightGrams { get; set; }

        /// <summary>Courier-applied / charged weight in grams.</summary>
        public decimal AppliedWeightGrams { get; set; }

        public decimal WeightCharges { get; set; }
        public DateTime WeightAppliedDate { get; set; } = DateTime.UtcNow;

        public WeightDiscrepancyStatus Status { get; set; } = WeightDiscrepancyStatus.ActionRequired;

        public string? DisputeRemarks { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? DisputedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? ClosedRemarks { get; set; }
    }
}
