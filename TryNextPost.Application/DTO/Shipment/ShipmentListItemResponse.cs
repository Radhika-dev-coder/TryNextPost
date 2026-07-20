namespace TryNextPost.Application.DTO.Shipment
{
    public class ShipmentListItemResponse
    {
        public long ShipmentId { get; set; }
        public long OrderId { get; set; }
        public string? OrderRef { get; set; }
        public string? AwbNumber { get; set; }
        public long CourierId { get; set; }
        public string? CourierCode { get; set; }
        public string? CourierName { get; set; }
        public string? ServiceCode { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int ShipmentType { get; set; }
        public string ShipmentTypeName { get; set; } = string.Empty;
        public decimal ChargedAmount { get; set; }
        public string DeliveryCustomerName { get; set; } = string.Empty;
        public string DeliveryPincode { get; set; } = string.Empty;
        public string DeliveryCity { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }
}
