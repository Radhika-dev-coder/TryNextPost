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
    public class RTO : BaseDbModel
    {
        [Key]
        public long RtoId { get; set; }

        public long ShipmentId { get; set; }
        public Shipment? Shipment { get; set; }

        public string Reason { get; set; } = string.Empty;

        public RtoStatus Status { get; set; } = RtoStatus.Initiated;

        
    }
}
