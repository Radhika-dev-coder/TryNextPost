namespace TryNextPost.Application.DTO.Payment
{
    public class RazorpayCreateOrderResult
    {
        public string OrderId { get; set; } = string.Empty;
        public int AmountInPaise { get; set; }
        public string Currency { get; set; } = "INR";
        public string Receipt { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
