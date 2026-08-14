using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class CourierPickupLocation : BaseDbModel
    {
        [Key]
        public long CourierPickupLocationId { get; set; }

        public long AddressId { get; set; }

        public long CourierId { get; set; }

        [MaxLength(13)]
        public string LocationCode { get; set; } = string.Empty;

        // Navigation
        public Address Address { get; set; } = null!;

        public Courier Courier { get; set; } = null!;
    }
}
