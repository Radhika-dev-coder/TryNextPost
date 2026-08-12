using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Constants
{
    public static class CustomReportFieldKeys
    {
        // ── Orders ──
        public const string OrderNumber = "orderNumber";
        public const string OrderDate = "orderDate";
        public const string OrderStatus = "orderStatus";
        public const string PaymentMode = "paymentMode";
        public const string OrderType = "orderType";
        public const string Channel = "channel";
        public const string CustomerName = "customerName";
        public const string CustomerMobile = "customerMobile";
        public const string ShippingCity = "shippingCity";
        public const string ShippingState = "shippingState";
        public const string ShippingPincode = "shippingPincode";
        public const string ProductNames = "productNames";
        public const string TotalAmount = "totalAmount";
        public const string FinalPayableAmount = "finalPayableAmount";
        public const string CodCharges = "codCharges";
        public const string ShippingCharges = "shippingCharges";
        public const string OrderWeightGrams = "orderWeightGrams";
        // ── Shipment ──
        public const string Awb = "awb";
        public const string ShipmentStatus = "shipmentStatus";
        public const string CourierName = "courierName";
        public const string ServiceCode = "serviceCode";
        public const string ChargedAmount = "chargedAmount";
        public const string ShipmentWeight = "shipmentWeight";
        // ── Warehouse (pickup) ──
        public const string WarehouseName = "warehouseName";
        public const string WarehouseCity = "warehouseCity";
        public const string WarehouseState = "warehouseState";
        public const string WarehousePincode = "warehousePincode";
        public const string WarehouseMobile = "warehouseMobile";
        public static readonly IReadOnlyList<string> All =
        [
            OrderNumber, OrderDate, OrderStatus, PaymentMode, OrderType, Channel,
            CustomerName, CustomerMobile, ShippingCity, ShippingState, ShippingPincode,
            ProductNames, TotalAmount, FinalPayableAmount, CodCharges, ShippingCharges, OrderWeightGrams,
            Awb, ShipmentStatus, CourierName, ServiceCode, ChargedAmount, ShipmentWeight,
            WarehouseName, WarehouseCity, WarehouseState, WarehousePincode, WarehouseMobile
        ];
        public static bool IsValid(string key) =>
            All.Contains(key, StringComparer.OrdinalIgnoreCase);
    }
}
