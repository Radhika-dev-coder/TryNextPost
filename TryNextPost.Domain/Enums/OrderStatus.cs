using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Enums
{
    public enum OrderStatus
    {
            Pending = 1,
            Confirmed = 2,
            Packed = 3,
            Shipped = 4,
            Delivered = 5,
            Cancelled = 6,
            RTO = 7
    }
}
