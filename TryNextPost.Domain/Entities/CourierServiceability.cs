using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Entities
{
    public class CourierServiceability
    {
        [Key]
        public long ServiceabilityId { get; set; } 

        // 🔗 FK → Courier
        public long CourierId { get; set; }
        // Navigation
        public Courier? Courier { get; set; }
        public string Pincode { get; set; } = string.Empty;
        public bool IsServiceable { get; set; }
        public int EstimatedDays { get; set; }

        
    }
}
