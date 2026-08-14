using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.XpressBees
{
    public class XpressBeesWeight
    {
        public string BillableWeight { get; set; } = string.Empty;
        public string PhyWeight { get; set; } = string.Empty;
        public string VolWeight { get; set; } = string.Empty;
    }
}
