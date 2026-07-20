namespace TryNextPost.Application.DTO.Shipment
{
    public class ShipmentTrackResponse
    {
        public long ShipmentId { get; set; }
        public string? AwbNumber { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? CourierCurrentStatus { get; set; }
        public bool IsStub { get; set; }
        public string? Message { get; set; }
        public List<ShipmentTrackEventDto> Events { get; set; } = new();
    }

    public class ShipmentTrackEventDto
    {
        public DateTime EventTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? StatusCode { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public bool FromLocalHistory { get; set; }
    }
}
