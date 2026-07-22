namespace TryNextPost.Application.DTO.Weight
{
    public class WeightFreezeListResponse
    {
        public List<WeightFreezeListItemResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public WeightFreezeTabCounts TabCounts { get; set; } = new();
    }
}
