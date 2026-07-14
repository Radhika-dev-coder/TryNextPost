using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Default
{
    public class AddressResponse
    {
        public long AddressId { get; set; }
        public string? WarehouseName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Mobile { get; set; } = string.Empty;
        public string? GstNumber { get; set; }
        public string Pincode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
}
