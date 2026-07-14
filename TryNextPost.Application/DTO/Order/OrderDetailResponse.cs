using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Order
{
    public class OrderDetailResponse
    {
        public long OrderId { get; set; }
        public string OrderRef { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal FinalPayableAmount { get; set; }

        public int PaymentMode { get; set; }
        public int OrderType { get; set; }
        public int OrderCategory { get; set; }
        public int Status { get; set; }

        public long BillingAddressId { get; set; }
        public string? GstNumber { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerCompanyName { get; set; }
        public string CustomerMobile { get; set; } = string.Empty;
        public string ShippingAddressLine1 { get; set; } = string.Empty;
        public string? ShippingAddressLine2 { get; set; }
        public string ShippingPincode { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingState { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;

        public decimal WeightGrams { get; set; }
        public decimal LengthCm { get; set; }
        public decimal BreadthCm { get; set; }
        public decimal HeightCm { get; set; }
        public decimal VolumetricWeightGrams { get; set; }

        public decimal ShippingCharges { get; set; }
        public decimal CodCharges { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public bool IsCollectableAmountDifferent { get; set; }
        public decimal? CollectableAmount { get; set; }

        public List<OrderItemDto> Items { get; set; } = new();
    }
}
