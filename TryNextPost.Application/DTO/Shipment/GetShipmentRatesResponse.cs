namespace TryNextPost.Application.DTO.Shipment
{
    public class GetShipmentRatesResponse
    {
        public long OrderId { get; set; }
        public string OrderRef { get; set; } = string.Empty;
        public string OriginPincode { get; set; } = string.Empty;
        public string DestinationPincode { get; set; } = string.Empty;
        public List<ShipmentRateOptionDto> Rates { get; set; } = new();
    }
}
