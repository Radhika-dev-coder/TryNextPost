using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Pincode
{
    public class LocationResponseDto
    {
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Pincode { get; set; }
        public string FullAddress { get; set; }
        public string Area { get; set; }
        public string Road { get; set; }
    }
}
