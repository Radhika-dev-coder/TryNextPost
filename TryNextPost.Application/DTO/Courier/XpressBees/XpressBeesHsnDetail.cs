using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.XpressBees
{
    public class XpressBeesHsnDetail
    {
        public string? ProductCategory { get; set; }
        public string? ProductDesc { get; set; }

        public decimal? CGSTAmount { get; set; }
        public decimal? Discount { get; set; }

        public decimal? GSTTAXRateIGSTN { get; set; }
        public decimal? GSTTaxRateCGSTN { get; set; }
        public decimal? GSTTaxRateSGSTN { get; set; }

        public decimal? GSTTaxTotal { get; set; }

        public string? HSNCode { get; set; }

        public decimal? IGSTAmount { get; set; }
        public decimal? ProductQuantity { get; set; }
        public decimal? SGSTAmount { get; set; }
        public decimal? TaxableValue { get; set; }
    }
}
