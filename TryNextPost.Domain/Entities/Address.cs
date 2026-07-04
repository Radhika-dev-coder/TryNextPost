using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities
{
    public class Address : BaseDbModel
    {
        public string AddressId { get; set; } = Guid.NewGuid().ToString();

        public AddressType AddressType { get; set; }

        // Only store IDs, NOT navigation objects from Identity
        public string? UserId { get; set; }

        public string? CompanyId { get; set; }
        public CompanyInfo? Company { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;

        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }

        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

    }
}
