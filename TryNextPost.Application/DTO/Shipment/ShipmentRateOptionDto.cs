namespace TryNextPost.Application.DTO.Shipment
{
    public class ShipmentRateOptionDto
    {
        public long CourierId { get; set; }
        public string CourierCode { get; set; } = string.Empty;
        public string CourierName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string? ServiceCode { get; set; }
        public decimal TotalCharge { get; set; }
        public decimal? CodCharge { get; set; }
        public int EstimatedDays { get; set; }
        public bool IsStub { get; set; }
        public string? Message { get; set; }
    }
}
