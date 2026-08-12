namespace TryNextPost.Application.DTO.Billing
{
    public class InvoiceListResponse
    {
        public List<InvoiceListItemResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
