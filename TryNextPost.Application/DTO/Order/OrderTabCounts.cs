using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Order
{
    public class OrderTabCounts
    {
        public int AllOrders { get; set; }
        public int NotShipped { get; set; }
        public int Booked { get; set; }
        public int Cancelled { get; set; }
        public int FulfilledOrders { get; set; }
    }
}
