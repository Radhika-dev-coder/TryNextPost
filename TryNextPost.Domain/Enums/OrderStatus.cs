using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Enums
{
    public enum OrderStatus
    {
            Pending,
            Confirmed,
            Packed,
            Shipped,
            Delivered,
            Cancelled,
            RTO
    }
}
