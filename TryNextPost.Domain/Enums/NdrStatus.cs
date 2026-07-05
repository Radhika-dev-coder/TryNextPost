using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Enums
{
    public enum NdrStatus
    {
        Pending = 1,
        ReattemptScheduled = 2,
        Delivered = 3,
        Cancelled = 4
    }
}
