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
    public class Order : BaseDbModel
    {
        [Key]
        public long OrderId { get; set; }
        public long SellerId { get; set; } 
        public string OrderRef { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public PaymentMode PaymentMode { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // 🔹 Billing address — seller/company ki Address table se (FK)
        public long BillingAddressId { get; set; } 

        // 🔹 Customer/Shipping info — EMBEDDED (no FK, snapshot at order time)
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerMobile { get; set; } = string.Empty;
        public string ShippingAddressLine1 { get; set; } = string.Empty;
        public string? ShippingAddressLine2 { get; set; }
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingState { get; set; } = string.Empty;
        public string ShippingPincode { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;

        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}

