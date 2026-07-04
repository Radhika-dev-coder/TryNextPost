using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities
{
    public class Shipment
    {
        public long ShipmentId { get; set; }

        public long OrderId { get; set; }
        public Order? Order { get; set; }

        public int CourierId { get; set; }
        public Courier? Courier { get; set; }

        public string AwbNumber { get; set; } = string.Empty;

        public ShipmentType ShipmentType { get; set; }

        public string PickupAddressId { get; set; } = string.Empty;
        public Address? PickupAddress { get; set; }

        public string DeliveryAddressId { get; set; } = string.Empty;
        public Address? DeliveryAddress { get; set; }

        public decimal Weight { get; set; }
        public decimal Length { get; set; }
        public decimal Breadth { get; set; }
        public decimal Height { get; set; }

        public ShipmentStatus Status { get; set; } = ShipmentStatus.Created;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Tracking list
        public ICollection<ShipmentTracking>? TrackingHistory { get; set; }
    }
}
