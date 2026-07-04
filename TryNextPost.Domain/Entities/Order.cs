using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities
{
    public class Order : BaseDbModel
    {
        public long OrderId { get; set; }

        public string SellerId { get; set; } = string.Empty; // SellerId = ApplicationUser.Id

        public string OrderRef { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        public PaymentMode PaymentMode { get; set; }

        public string ShippingAddressId { get; set; } = string.Empty;
        public string BillingAddressId { get; set; } = string.Empty;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}

