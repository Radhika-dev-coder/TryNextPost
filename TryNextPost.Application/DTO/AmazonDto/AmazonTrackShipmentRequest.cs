using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.AmazonDto
{
    public class AmazonTrackShipmentRequest
    {
        public string ShipmentId { get; set; } = string.Empty;
        public string? TrackingId { get; set; }
    }
}
