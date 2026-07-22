namespace TryNextPost.Application.DTO.Ndr
{
    public class NdrListResponse
    {
        public List<NdrListItemResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public NdrTabCounts TabCounts { get; set; } = new();
    }
}
