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
    public class Courier : BaseDbModel
    {
        [Key]
        public long CourierId { get; set; } 
        public string CourierName { get; set; } = string.Empty;

        // API Integration details
        public string? ApiBaseUrl { get; set; }
        public string? ApiKey { get; set; }       // ⚠️ encrypt before storing
        public string? ApiSecret { get; set; }    // ⚠️ encrypt before storing
        public string? AccountCode { get; set; }

        // Contact details
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }

        // Business rules
        public bool SupportsCOD { get; set; }
        public bool SupportsPrepaid { get; set; }
        public decimal? MaxWeightLimit { get; set; }

        // Navigation
        public ICollection<Shipment>? Shipments { get; set; }
        public ICollection<CourierServiceability>? Serviceabilities { get; set; }
    }
}
