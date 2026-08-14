using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.XpressBees
{
    public class XpressBeesGetAwbSeriesRequest
    {
        public string BusinessUnit { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string BatchID { get; set; } = string.Empty;
    }
}
