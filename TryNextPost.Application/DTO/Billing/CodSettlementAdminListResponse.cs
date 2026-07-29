namespace TryNextPost.Application.DTO.Billing
{
    public class CodSettlementAdminListResponse
    {
        public List<CodRemittanceListItemResponse> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public decimal PendingTotal { get; set; }
        public decimal SettledTotal { get; set; }
    }
}
