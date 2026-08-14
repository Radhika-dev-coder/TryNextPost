using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.XpressBees
{
    public class XpressBeesPickupDetails
    {
        public List<XpressBeesAddress> Addresses { get; set; } = new();

        public List<XpressBeesContactDetails> ContactDetails { get; set; } = new();

        public string? PickupVendorCode { get; set; }

        public bool? IsGenSecurityCode { get; set; }
        public string? SecurityCode { get; set; }

        public bool? IsGeoFencingEnabled { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public decimal? MaxThresholdRadius { get; set; }
        public string? MidPoint { get; set; }
        public decimal? MinThresholdRadius { get; set; }
        public string? RediusLocation { get; set; }
    }
}
