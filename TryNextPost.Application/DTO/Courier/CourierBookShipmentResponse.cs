namespace TryNextPost.Application.DTO.Courier
{
    public class CourierBookShipmentResponse
    {
        public bool Success { get; set; }
        public bool IsStub { get; set; }
        public string CourierCode { get; set; } = string.Empty;
        public string? AwbNumber { get; set; }
        public string? CourierReference { get; set; }
        public string? LabelUrl { get; set; }
        public string? Message { get; set; }
    }
}
