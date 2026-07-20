namespace TryNextPost.Application.DTO.Courier
{
    public class CourierCancelResponse
    {
        public bool Success { get; set; }
        public bool IsStub { get; set; }
        public string CourierCode { get; set; } = string.Empty;
        public string? Message { get; set; }
    }
}
