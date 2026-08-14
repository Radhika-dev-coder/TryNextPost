using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.XpressBees
{
    public class XpressBeesManifestResponse
    {
        public string? AWBNo { get; set; }
        public int ReturnCode { get; set; }
        public string? ReturnMessage { get; set; }
        public string? TokenNumber { get; set; }
    }
}
