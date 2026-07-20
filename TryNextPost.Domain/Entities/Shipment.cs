using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities
{
    public class Shipment : BaseDbModel
    {
        [Key]
        public long ShipmentId { get; set; }

        // 🔗 FK → Order
        public long OrderId { get; set; }
        public Order? Order { get; set; }

        // 🔗 FK → Courier
        public long CourierId { get; set; }
        public Courier? Courier { get; set; }

        /// <summary>
        /// Courier AWB / tracking number. Nullable until booking succeeds; stub AWBs are alphanumeric.
        /// </summary>
        [MaxLength(100)]
        public string? AwbNumber { get; set; }

        /// <summary>Courier-side booking reference (if any).</summary>
        [MaxLength(100)]
        public string? CourierReference { get; set; }

        /// <summary>Selected rate service code (e.g. DELHIVERY_SURFACE_STUB).</summary>
        [MaxLength(100)]
        public string? ServiceCode { get; set; }

        public string? LabelUrl { get; set; }

        /// <summary>Freight amount charged / to be debit from wallet at booking.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ChargedAmount { get; set; }

        public ShipmentType ShipmentType { get; set; }

        // 🔹 Pickup — seller/warehouse address, FK to Address
        public long PickupAddressId { get; set; }
        public Address? PickupAddress { get; set; }

        // 🔹 Delivery — customer address snapshot (no FK)
        public string DeliveryCustomerName { get; set; } = string.Empty;
        public string DeliveryMobile { get; set; } = string.Empty;
        public string DeliveryAddressLine1 { get; set; } = string.Empty;
        public string? DeliveryAddressLine2 { get; set; }
        public string DeliveryCity { get; set; } = string.Empty;
        public string DeliveryState { get; set; } = string.Empty;
        public string DeliveryPincode { get; set; } = string.Empty;
        public string DeliveryCountry { get; set; } = string.Empty;

        public decimal Weight { get; set; }
        public decimal Length { get; set; }
        public decimal Breadth { get; set; }
        public decimal Height { get; set; }

        public ShipmentStatus Status { get; set; } = ShipmentStatus.Booked;

        public ICollection<ShipmentTracking>? TrackingHistory { get; set; }
        public ICollection<NDR>? NDRs { get; set; }
        public ICollection<RTO>? RTOs { get; set; }
    }
}
