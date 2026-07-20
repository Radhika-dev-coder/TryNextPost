using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Order
{
    public class OrderListItemResponse
    {
        public long OrderId { get; set; }
        public string Channel { get; set; } = "Manual";
        public string OrderRef { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string ProductSummary { get; set; } = string.Empty;
        public string PaymentMode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerMobile { get; set; } = string.Empty;
        public decimal WeightGrams { get; set; }
        public string? IvrStatus { get; set; }
        public string? WhatsAppStatus { get; set; }
        public string? ShopifyTags { get; set; }
        public string? Tags { get; set; }

        /// <summary>Order status name (e.g. Pending, Confirmed). Ship Now when Pending + CanShip.</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Numeric OrderStatus enum value.</summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// True when order is Pending (Not Shipped) and has no active shipment.
        /// Frontend should show Ship Now only when this is true.
        /// </summary>
        public bool CanShip { get; set; }

        /// <summary>True when an active (non-cancelled / non-booking-failed) shipment exists.</summary>
        public bool HasShipment { get; set; }

        public long? ShipmentId { get; set; }
        public string? AwbNumber { get; set; }
        public string? ShipmentStatus { get; set; }
        public string? CourierName { get; set; }
    }
}
