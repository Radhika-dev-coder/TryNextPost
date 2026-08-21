using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.AmazonDto
{
    public class AmazonCreateShipmentResponse
    {
        public AmazonCreateShipmentPayload Payload { get; set; } = new();
    }

    public class AmazonCreateShipmentPayload
    {
        public string? ShipmentId { get; set; }

        public string? TrackingId { get; set; }

        public string? ServiceId { get; set; }

        public string? LabelUrl { get; set; }

        public string? ShipmentStatus { get; set; }
    }
}
