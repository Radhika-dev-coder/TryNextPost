using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Order
{
    public class UpdateForwardOrderRequest
    {
        public int PaymentMode { get; set; }
        public string? GstNumber { get; set; }

        public long? PickupAddressId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerCompanyName { get; set; }
        public string CustomerMobile { get; set; } = string.Empty;
        public string ShippingAddressLine1 { get; set; } = string.Empty;
        public string? ShippingAddressLine2 { get; set; }
        public string ShippingPincode { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingState { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;

        public bool IsBillingSameAsShipping { get; set; } = true;
        public string? BillingFirstName { get; set; }
        public string? BillingLastName { get; set; }
        public string? BillingCompanyName { get; set; }
        public string? BillingAddressLine1 { get; set; }
        public string? BillingAddressLine2 { get; set; }
        public string? BillingCity { get; set; }
        public string? BillingState { get; set; }
        public string? BillingPincode { get; set; }
        public string? BillingCountry { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();

        public decimal WeightGrams { get; set; }
        public decimal LengthCm { get; set; }
        public decimal BreadthCm { get; set; }
        public decimal HeightCm { get; set; }

        public decimal ShippingCharges { get; set; }
        public decimal CodCharges { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public bool IsCollectableAmountDifferent { get; set; }
        public decimal? CollectableAmount { get; set; }
    }
}
