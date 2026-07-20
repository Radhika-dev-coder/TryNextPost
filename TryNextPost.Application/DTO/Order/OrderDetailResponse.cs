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

        /// <summary>Order status name (e.g. Pending). Useful alongside numeric Status.</summary>
        public string StatusName { get; set; } = string.Empty;

        /// <summary>
        /// True when order is Pending and has no active shipment — show Ship Now.
        /// </summary>
        public bool CanShip { get; set; }

        /// <summary>True when an active (non-cancelled / non-booking-failed) shipment exists.</summary>
        public bool HasShipment { get; set; }

        public long? ShipmentId { get; set; }
        public string? AwbNumber { get; set; }
        public string? ShipmentStatus { get; set; }
        public string? CourierName { get; set; }

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

<<<<<<< Updated upstream
        public long?   PickupAddressId { get; set; }
        public bool    IsBillingSameAsShipping { get; set; }
=======

        public long? PickupAddressId { get; set; }
        public bool IsBillingSameAsShipping { get; set; }
>>>>>>> Stashed changes
        public string? BillingFirstName { get; set; }
        public string? BillingLastName { get; set; }
        public string? BillingCompanyName { get; set; }
        public string? BillingAddressLine1 { get; set; }
        public string? BillingAddressLine2 { get; set; }
        public string? BillingCity { get; set; }
        public string? BillingState { get; set; }
        public string? BillingPincode { get; set; }
        public string? BillingCountry { get; set; }

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
        public ReverseQcDetailResponse? ReverseQcDetail { get; set; }
    }
}
