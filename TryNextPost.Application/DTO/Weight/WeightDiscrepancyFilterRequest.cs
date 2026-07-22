namespace TryNextPost.Application.DTO.Weight
{
    public class WeightDiscrepancyFilterRequest
    {
        public string? StatusTab { get; set; } = "all";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        /// <summary>Comma-separated AWB numbers.</summary>
        public string? AwbNumbers { get; set; }

        public string? ProductName { get; set; }
        public long? CourierId { get; set; }
    }
}
