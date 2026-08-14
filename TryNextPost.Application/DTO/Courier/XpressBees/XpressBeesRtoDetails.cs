using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.XpressBees
{
    public class XpressBeesRtoDetails
    {
        public List<XpressBeesAddress> Addresses { get; set; } = new();

        public List<XpressBeesContactDetails> ContactDetails { get; set; } = new();
    }
}
