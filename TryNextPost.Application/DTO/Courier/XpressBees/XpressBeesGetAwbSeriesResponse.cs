using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.XpressBees
{
    public class XpressBeesGetAwbSeriesResponse
    {
        public string? ReturnMessage { get; set; }
        public int ReturnCode { get; set; }
        public string? AWBNoGenRequestedDateTime { get; set; }
        public string? BatchID { get; set; }
        public List<string>? AWBNoSeries { get; set; }
    }
}
