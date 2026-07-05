using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Enums
{
    public enum RtoStatus
    {
        Initiated = 1,
        InTransit = 2,
        DeliveredToSeller = 3,
        Cancelled = 4
    }
}
