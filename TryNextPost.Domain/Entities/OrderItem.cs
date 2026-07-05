using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class OrderItem : BaseDbModel
    {
        [Key]
        public long OrderItemId { get; set; }
        public long OrderId { get; set; }
        public Order? Order { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Qty { get; set; }
        public decimal Price { get; set; }

        [NotMapped]
        public decimal TotalPrice => Qty * Price;
    }
}
