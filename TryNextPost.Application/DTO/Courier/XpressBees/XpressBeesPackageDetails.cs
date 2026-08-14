using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.XpressBees
{
    public class XpressBeesPackageDetails
    {
        public XpressBeesDimensions Dimensions { get; set; } = new();

        public XpressBeesWeight Weight { get; set; } = new();
    }
}
