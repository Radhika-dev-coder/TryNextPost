using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities
{
    public class Courier : BaseDbModel
    {
        public int CourierId { get; set; }

        public string CourierName { get; set; } = string.Empty;

        // API Integration details (IMPORTANT for NimbusPost type system)
        public string? ApiBaseUrl { get; set; }

        public string? ApiKey { get; set; }

        public string? ApiSecret { get; set; }

        public string? AccountCode { get; set; }

        // Contact details
        public string? ContactEmail { get; set; }

        public string? ContactPhone { get; set; }

        // Business rules
        public bool SupportsCOD { get; set; }

        public bool SupportsPrepaid { get; set; }

        public decimal? MaxWeightLimit { get; set; }

        //public CourierStatus Status { get; set; } = CourierStatus.Active;

        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //public DateTime? UpdatedAt { get; set; }

        // Navigation (optional but useful)
        public ICollection<Shipment>? Shipments { get; set; }

        public ICollection<CourierServiceability>? Serviceabilities { get; set; }
    }
}
