namespace TryNextPost.Application.DTO.Courier
{
    public class CourierCancelRequest
    {
        public string AwbNumber { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }
}
