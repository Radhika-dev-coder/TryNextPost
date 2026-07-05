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
    public class Address : BaseDbModel
    {
        [Key]
        public long AddressId { get; set; }
        public AddressType AddressType { get; set; }   // Pickup, Warehouse, Billing
        public string? UserId { get; set; }              // Seller ka address
        public long CompanyId { get; set; }   // FK

        public CompanyInfo Company { get; set; }            // Company ka billing address
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
