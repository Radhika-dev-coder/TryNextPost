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
        [MaxLength(50)]
        public string CourierCode { get; set; } = string.Empty;

        // API Integration details
        public string? ApiBaseUrl { get; set; }
        public string? ApiKey { get; set; }       //  encrypt before storing
        public string? ApiSecret { get; set; }    //  encrypt before storing
        public string? AccountCode { get; set; }


        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }

        public bool SupportsCOD { get; set; }
        public bool SupportsPrepaid { get; set; }
        public decimal? MaxWeightLimit { get; set; }

        public CodChargeType CodChargeType { get; set; } = CodChargeType.Flat;

        public decimal CodChargeValue { get; set; } = 30m;

        // Navigation
        public ICollection<Shipment>? Shipments { get; set; }
        public ICollection<CourierServiceability>? Serviceabilities { get; set; }
        public ICollection<PincodeZoneMapping> PincodeZoneMappings { get; set; }
    }
}
