using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Order
{
    public sealed class OrderSummaryDetailDto
    {
        // Order Profile
        public long OrderId { get; set; }
        public string OrderRef { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string PaymentModeName { get; set; } = string.Empty;
        public decimal FinalPayableAmount { get; set; }
        public decimal? CollectableAmount { get; set; } // Added for COD fields

        // Customer Shipping Details
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerMobile { get; set; } = string.Empty;
        public string CompleteShippingAddress { get; set; } = string.Empty;

        // NEW: Billing Details Snapshot
        public string BillingDetailsText { get; set; } = string.Empty;

        // NEW: Warehouse Details Snapshot
        public string WarehouseName { get; set; } = string.Empty;
        public string WarehouseMobile { get; set; } = string.Empty;
        public string WarehouseCompleteAddress { get; set; } = string.Empty;

        // Weight & Dimensions Details
        public decimal PackageWeightKg { get; set; }
        public string DimensionsText { get; set; } = string.Empty;
        public decimal VolumetricWeightGrams { get; set; } // Added

        // Courier Allocation Info
        public string? ActiveAwbNumber { get; set; }
        public string? AllocatedCourierName { get; set; }
        public decimal ShippingCharges { get; set; }

        // Collections
        public List<OrderSummaryItemDto> LineItems { get; set; } = new();
    }
}
