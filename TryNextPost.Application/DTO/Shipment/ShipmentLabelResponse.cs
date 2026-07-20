namespace TryNextPost.Application.DTO.Shipment
{
    public class ShipmentLabelResponse
    {
        public long ShipmentId { get; set; }
        public string? AwbNumber { get; set; }
        public string? LabelUrl { get; set; }
        public string? ContentType { get; set; }
        public string? LabelBase64 { get; set; }
        public bool IsStub { get; set; }
        public string? Message { get; set; }
    }
}
