namespace TryNextPost.Application.DTO.Billing
{
    public class CodRemittanceListResponse
    {
        public List<CodRemittanceListItemResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
