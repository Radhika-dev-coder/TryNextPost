using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public long AwbNumber { get; set; }
        public ShipmentType ShipmentType { get; set; }

        // 🔹 Pickup — seller/warehouse ka address, FK to Address table
        public long PickupAddressId { get; set; }
        public Address? PickupAddress { get; set; }

        // 🔹 Delivery — customer ka address, EMBEDDED (snapshot, no FK)
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

        public ShipmentStatus Status { get; set; } = ShipmentStatus.Created;

        // Tracking list
        public ICollection<ShipmentTracking>? TrackingHistory { get; set; }
        public ICollection<NDR>? NDRs { get; set; }   
        public ICollection<RTO>? RTOs { get; set; }
    }
}
