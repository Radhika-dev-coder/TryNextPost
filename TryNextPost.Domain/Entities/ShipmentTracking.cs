using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Entities
{
    public class ShipmentTracking
    {
        public long TrackingId { get; set; }

        public long ShipmentId { get; set; }
        public Shipment? Shipment { get; set; }

        public string Status { get; set; } = string.Empty;

        public string StatusCode { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime EventTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
