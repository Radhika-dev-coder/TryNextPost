using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.XpressBees
{
    public class XpressBeesServiceabilityRequest
    {
        public string BusinessUnit { get; set; } = string.Empty;
        public string BusinessFlow { get; set; } = string.Empty;
        public string BusinessService { get; set; } = string.Empty;
    }
}
