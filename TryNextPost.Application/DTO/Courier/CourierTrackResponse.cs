namespace TryNextPost.Application.DTO.Courier
{
    public class CourierTrackResponse
    {
        public bool Success { get; set; }
        public bool IsStub { get; set; }
        public string CourierCode { get; set; } = string.Empty;
        public string? AwbNumber { get; set; }
        public string? CurrentStatus { get; set; }
        public string? Message { get; set; }
        public List<CourierTrackEvent> Events { get; set; } = new();
    }

    public class CourierTrackEvent
    {
        public DateTime EventTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? StatusCode { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
    }
}
