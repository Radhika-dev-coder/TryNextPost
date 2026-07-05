using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities
{
    public class CODSettlement : BaseDbModel
    {
        [Key]
        public long CodSettlementId { get; set; } 
        public long ShipmentId { get; set; }
        public long SellerId { get; set; } 
        public decimal CodAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public DateTime? SettlementDate { get; set; }
        public SettlementStatus Status { get; set; } = SettlementStatus.Pending;
        public Shipment? Shipment { get; set; }
    }
}
