using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Courier.XpressBees
{
    public class XpressBeesManifestRequest
    {
        public string AirWayBillNO { get; set; } = string.Empty;
        public string BusinessAccountName { get; set; } = string.Empty;
        public string OrderNo { get; set; } = string.Empty;
        public string? SubOrderNo { get; set; }


        public string OrderDate { get; set; } = string.Empty;
        public string OrderType { get; set; } = string.Empty;
        public string CollectibleAmount { get; set; } = string.Empty;
        public string DeclaredValue { get; set; } = string.Empty;

        public string PickupType { get; set; } = string.Empty;
        public string Quantity { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;

        public string ProductDescription { get; set; } = string.Empty;

        public XpressBeesDropDetails DropDetails { get; set; } = new();
        public XpressBeesPickupDetails PickupDetails { get; set; } = new();
        public XpressBeesRtoDetails RTODetails { get; set; } = new();

        public string Instruction { get; set; } = string.Empty;
        public string? CustomerPromiseDate { get; set; }

        public bool? IsCommercialProperty { get; set; }
        public bool? IsDGShipmentType { get; set; }
        public bool? IsOpenDelivery { get; set; }
        public bool? IsSameDayDelivery { get; set; }

        public string ManifestID { get; set; } = string.Empty;

        public string? MultiShipmentGroupID { get; set; }
        public string? SenderName { get; set; }

        public bool? IsEssential { get; set; }
        public bool? IsSecondaryPacking { get; set; }

        public XpressBeesPackageDetails PackageDetails { get; set; } = new();

        public List<XpressBeesGstMultiSellerInfo> GSTMultiSellerInfo { get; set; } = new();
    }
}
