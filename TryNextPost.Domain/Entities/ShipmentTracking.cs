using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities
{
    public class ShipmentTracking
    {
        [Key]
        public long TrackingId { get; set; }

        // 🔗 FK → Shipment
        public long ShipmentId { get; set; }
        public Shipment? Shipment { get; set; }

        public ShipmentStatus Status { get; set; }               
        public string StatusCode { get; set; } = string.Empty;   

        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public DateTime EventTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
