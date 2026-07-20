namespace TryNextPost.Application.DTO.Courier
{
    public class CourierBookShipmentRequest
    {
        public string OrderRef { get; set; } = string.Empty;
        public string? ServiceCode { get; set; }

        public string PickupName { get; set; } = string.Empty;
        public string PickupPhone { get; set; } = string.Empty;
        public string PickupAddressLine1 { get; set; } = string.Empty;
        public string? PickupAddressLine2 { get; set; }
        public string PickupCity { get; set; } = string.Empty;
        public string PickupState { get; set; } = string.Empty;
        public string PickupPincode { get; set; } = string.Empty;
        public string PickupCountry { get; set; } = "India";

        public string DeliveryName { get; set; } = string.Empty;
        public string DeliveryPhone { get; set; } = string.Empty;
        public string DeliveryAddressLine1 { get; set; } = string.Empty;
        public string? DeliveryAddressLine2 { get; set; }
        public string DeliveryCity { get; set; } = string.Empty;
        public string DeliveryState { get; set; } = string.Empty;
        public string DeliveryPincode { get; set; } = string.Empty;
        public string DeliveryCountry { get; set; } = "India";

        public decimal WeightKg { get; set; }
        public decimal? LengthCm { get; set; }
        public decimal? BreadthCm { get; set; }
        public decimal? HeightCm { get; set; }

        public bool IsCod { get; set; }
        public decimal? CodAmount { get; set; }
        public decimal? InvoiceValue { get; set; }
        public string? ProductDescription { get; set; }
    }
}
