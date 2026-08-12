namespace TryNextPost.Application.DTO.Weight
{
    public class WeightFreezeFilterRequest
    {
        public string? StatusTab { get; set; } = "all";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;

        /// <summary>Search by product name or SKU.</summary>
        public string? ProductSearch { get; set; }

        /// <summary>Exact/partial PID match.</summary>
        public string? ProductId { get; set; }
    }
}
