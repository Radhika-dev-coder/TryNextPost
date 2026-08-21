using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.AmazonDto
{
    public class AmazonBookShipmentResponse
    {
        public AmazonBookShipmentPayload Payload { get; set; } = new();
    }

    public class AmazonBookShipmentPayload
    {
        public string? ShipmentId { get; set; }

        public string? TrackingId { get; set; }

        public string? CarrierName { get; set; }

        public string? ServiceName { get; set; }

        public AmazonLabelInfo? Label { get; set; }

        public string? Status { get; set; }
    }

    public class AmazonLabelInfo
    {
        public string? LabelFormat { get; set; }

        public string? LabelData { get; set; }

        public string? LabelUrl { get; set; }
    }
}
