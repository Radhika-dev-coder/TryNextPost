using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities
{
    public class CODSettlement
    {
        public string CodSettlementId { get; set; } = Guid.NewGuid().ToString();

        // 🔗 FK → Shipment
        public string ShipmentId { get; set; } = string.Empty;

        // 🔗 FK → Seller (UserId)
        public string SellerId { get; set; } = string.Empty;

        public decimal CodAmount { get; set; }

        public decimal CollectedAmount { get; set; }

        public DateTime? SettlementDate { get; set; }

        public SettlementStatus Status { get; set; } = SettlementStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optional Navigation
        public Shipment? Shipment { get; set; }
    }
}
