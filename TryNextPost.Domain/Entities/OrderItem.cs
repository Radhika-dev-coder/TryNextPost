using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Entities
{
    public class OrderItem
    {
        public long OrderItemId { get; set; }

        public long OrderId { get; set; }
        public Order? Order { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public int Qty { get; set; }

        public decimal Price { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
