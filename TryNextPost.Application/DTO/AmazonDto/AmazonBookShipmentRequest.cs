using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.AmazonDto
{
    public class AmazonBookShipmentRequest
    {
        public string OrderRef { get; set; } = string.Empty;

        public AmazonAddress ShipFrom { get; set; } = new();

        public AmazonAddress ShipTo { get; set; } = new();

        public List<AmazonPackage> Packages { get; set; } = [];

        public AmazonChannelDetails ChannelDetails { get; set; } = new();

        public string? ServiceId { get; set; }

        public AmazonMoney? InvoiceValue { get; set; }
    }
}
