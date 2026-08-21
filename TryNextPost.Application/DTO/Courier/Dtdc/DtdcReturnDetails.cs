using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.Dtdc
{
    public class DtdcReturnDetails
    {
        public string? Name { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? Email { get; set; }
        public string? CityName { get; set; }
        public string? Phone { get; set; }
        public string? Pincode { get; set; }
        public string? StateName { get; set; }
        public string? AlternatePhone { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
    }
}
