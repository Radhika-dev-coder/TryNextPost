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
    public class NDR : BaseDbModel
    {
        [Key]
        public long NdrId { get; set; }

        // 🔗 FK → Shipment
        public long ShipmentId { get; set; }
        public Shipment? Shipment { get; set; }

        public string Reason { get; set; } = string.Empty;
        public int Attempts { get; set; } = 0;
        public NdrStatus Status { get; set; } = NdrStatus.Pending;
        public string? Action { get; set; }
        public DateTime? NextAttemptDate { get; set; }
        public string? Remarks { get; set; }
    }
}
