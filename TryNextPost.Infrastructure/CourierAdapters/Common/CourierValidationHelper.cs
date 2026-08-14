using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Infrastructure.CourierAdapters.Common
{
    public static class CourierValidationHelper
    {
        public static bool IsPincodeServiceable(
    IEnumerable<int> serviceablePincodes,
    string pincode)
        {
            return serviceablePincodes.Any(x => x.ToString() == pincode);
        }
    }
}
