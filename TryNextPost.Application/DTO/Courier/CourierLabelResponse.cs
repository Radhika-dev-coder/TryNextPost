namespace TryNextPost.Application.DTO.Courier
{
    public class CourierLabelResponse
    {
        public bool Success { get; set; }
        public bool IsStub { get; set; }
        public string CourierCode { get; set; } = string.Empty;
        public string? LabelUrl { get; set; }
        public byte[]? LabelContent { get; set; }
        public string? ContentType { get; set; }
        public string? Message { get; set; }
    }
}
