using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.AmazonDto
{
    public class AmazonCancelShipmentResponse
    {
        public AmazonCancelShipmentPayload Payload { get; set; } = new();
    }

    public class AmazonCancelShipmentPayload
    {
        public string? ShipmentId { get; set; }
        public string? TrackingId { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }
    }
}
