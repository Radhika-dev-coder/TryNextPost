using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.XpressBees
{
    public class XpressBeesGstMultiSellerInfo
    {
        public string? BuyerGSTRegNumber { get; set; }
        public string? EBNExpiryDate { get; set; }
        public string? EWayBillSrNumber { get; set; }
        public string? InvoiceDate { get; set; }
        public string? InvoiceNumber { get; set; }
        public decimal? InvoiceValue { get; set; }

        public string? IsSellerRegUnderGST { get; set; }

        public string? ProductUniqueID { get; set; }
        public string? SellerAddress { get; set; }
        public string? SellerGSTRegNumber { get; set; }
        public string? SellerName { get; set; }
        public string? SellerPincode { get; set; }
        public string? SupplySellerStatePlace { get; set; }

        public List<XpressBeesHsnDetail> HSNDetails { get; set; } = new();
    }
}
