namespace TryNextPost.Application.DTO.Billing
{
    public class ShipmentChargesFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        /// <summary>Comma-separated AWB numbers.</summary>
        public string? AwbNumbers { get; set; }
    }
}
