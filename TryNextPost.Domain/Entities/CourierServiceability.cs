using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Entities
{
    public class CourierServiceability
    {
        // 🔗 FK → Courier
        public int CourierId { get; set; }

        public string Pincode { get; set; } = string.Empty;

        public bool IsServiceable { get; set; }

        public int EstimatedDays { get; set; }

        // ❗ Navigation (optional but useful)
        public Courier? Courier { get; set; }
    }
}
