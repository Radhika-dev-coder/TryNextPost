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
        public AddressType AddressType { get; set; }
        public string? UserId { get; set; }
        public long? CompanyId { get; set; }
        public CompanyInfo Company { get; set; }            // Company ka billing address
        public string? WarehouseName { get; set; }      // 👈 NAYA
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }                 // 👈 NAYA
        public string Mobile { get; set; } = string.Empty;
        public string? GstNumber { get; set; }             // 👈 NAYA

        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;


    }
}
