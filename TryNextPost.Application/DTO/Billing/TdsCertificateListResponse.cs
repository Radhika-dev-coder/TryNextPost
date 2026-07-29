namespace TryNextPost.Application.DTO.Billing
{
    public class TdsCertificateListResponse
    {
        public List<TdsCertificateListItemResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
