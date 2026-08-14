using TryNextPost.Domain.Enums;

namespace TryNextPost.Application.DTO.Courier
{
    public class CourierBookShipmentRequest
    {
        public long OrderId { get; set; }
        public string OrderRef { get; set; } = string.Empty;

        public string? ServiceCode { get; set; }

        public string ServiceType { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public string PickupType { get; set; } = string.Empty;

        public string? PickupVendorCode { get; set; }

        public long AddressId { get; set; }

        // Pickup
        public string PickupName { get; set; } = string.Empty;
        public string PickupPhone { get; set; } = string.Empty;
        public string PickupAddressLine1 { get; set; } = string.Empty;
        public string? PickupAddressLine2 { get; set; }
        public string PickupCity { get; set; } = string.Empty;
        public string PickupState { get; set; } = string.Empty;
        public string PickupPincode { get; set; } = string.Empty;
        public string PickupCountry { get; set; } = "India";

        // Delivery
        public string DeliveryName { get; set; } = string.Empty;
        public string DeliveryPhone { get; set; } = string.Empty;
        public string DeliveryAddressLine1 { get; set; } = string.Empty;
        public string? DeliveryAddressLine2 { get; set; }
        public string DeliveryCity { get; set; } = string.Empty;
        public string DeliveryState { get; set; } = string.Empty;
        public string DeliveryPincode { get; set; } = string.Empty;
        public string DeliveryCountry { get; set; } = "India";

        // RTO
        public string RtoAddressLine1 { get; set; } = string.Empty;
        public string? RtoAddressLine2 { get; set; }
        public string RtoCity { get; set; } = string.Empty;
        public string RtoState { get; set; } = string.Empty;
        public string RtoPincode { get; set; } = string.Empty;
        public string RtoCountry { get; set; } = "India";

        // Package
        public decimal WeightKg { get; set; }
        public decimal? LengthCm { get; set; }
        public decimal? BreadthCm { get; set; }
        public decimal? HeightCm { get; set; }

        // Payment
        public bool IsCod { get; set; }
        public decimal? CodAmount { get; set; }
        public decimal? InvoiceValue { get; set; }

        public string? ProductDescription { get; set; }

        public OrderTypeEnum OrderType { get; set; }
    }
}
