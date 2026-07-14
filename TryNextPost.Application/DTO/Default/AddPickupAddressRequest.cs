using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Default
{
    public class AddPickupAddressRequest
    {
        [Required(ErrorMessage = "Warehouse Name is required")]
        public string WarehouseName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact Name is required")]
        public string ContactName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Contact Number is required")]
        [Phone(ErrorMessage = "Invalid contact number")]
        public string ContactNumber { get; set; } = string.Empty;

        public string? GstNumber { get; set; }

        [Required(ErrorMessage = "Pincode is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be 6 digits")]
        public string Pincode { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address details are required")]
        public string AddressDetails { get; set; } = string.Empty;
    }
}
