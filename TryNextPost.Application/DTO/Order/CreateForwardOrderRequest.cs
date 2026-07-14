using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Order
{
    public class CreateForwardOrderRequest
    {
        public string? OrderRef { get; set; }      

        [Required]
        [Range(1, 2, ErrorMessage = "Invalid Payment Mode")]
        public int PaymentMode { get; set; }

        [Required]
        public long BillingAddressId { get; set; }

        public string? GstNumber { get; set; }

        [Required(ErrorMessage = "Customer Name is required")]
        public string CustomerName { get; set; } = string.Empty;

        public string? CustomerCompanyName { get; set; }

        [Required(ErrorMessage = "Customer Mobile is required")]
        [Phone(ErrorMessage = "Invalid mobile number")]
        public string CustomerMobile { get; set; } = string.Empty;

        [Required(ErrorMessage = "Shipping Address is required")]
        public string ShippingAddressLine1 { get; set; } = string.Empty;

        public string? ShippingAddressLine2 { get; set; }

        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be 6 digits")]
        public string ShippingPincode { get; set; } = string.Empty;

        [Required]
        public string ShippingCity { get; set; } = string.Empty;

        [Required]
        public string ShippingState { get; set; } = string.Empty;

        [Required]
        public string ShippingCountry { get; set; } = string.Empty;

        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<OrderItemDto> Items { get; set; } = new();

        [Range(1, double.MaxValue, ErrorMessage = "Weight must be greater than 0")]
        public decimal WeightGrams { get; set; }

        [Range(0.1, double.MaxValue, ErrorMessage = "Length must be greater than 0")]
        public decimal LengthCm { get; set; }

        [Range(0.1, double.MaxValue, ErrorMessage = "Breadth must be greater than 0")]
        public decimal BreadthCm { get; set; }

        [Range(0.1, double.MaxValue, ErrorMessage = "Height must be greater than 0")]
        public decimal HeightCm { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ShippingCharges { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CodCharges { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TaxAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Discount { get; set; }

        public bool IsCollectableAmountDifferent { get; set; }
        public decimal? CollectableAmount { get; set; }
    }
}
