using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Enums
{
    public enum ShipmentStatus
    {
        Created = 1,
        PickedUp = 2,
        InTransit = 3,
        OutForDelivery = 4,
        Delivered  = 5,
        RTO = 6
    }
}
