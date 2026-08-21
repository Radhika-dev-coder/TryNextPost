using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.AmazonDto
{
    public class AmazonTrackShipmentResponse
    {
        public AmazonTrackShipmentPayload Payload { get; set; } = new();
    }

    public class AmazonTrackShipmentPayload
    {
        public string? ShipmentId { get; set; }
        public string? TrackingId { get; set; }
        public string? Status { get; set; }
        public string? StatusCode { get; set; }
        public List<AmazonTrackingEvent> Events { get; set; } = [];
    }

    public class AmazonTrackingEvent
    {
        public string? Status { get; set; }
        public string? StatusCode { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public DateTime? EventTime { get; set; }
    }
}
