namespace TryNextPost.Application.DTO.Weight
{
    public class WeightDiscrepancyListResponse
    {
        public List<WeightDiscrepancyListItemResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public WeightDiscrepancyTabCounts TabCounts { get; set; } = new();
    }
}
