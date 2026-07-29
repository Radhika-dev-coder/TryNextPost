namespace TryNextPost.Application.DTO.Billing
{
    public class TdsCertificateFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string? FinancialYear { get; set; }
        public string? Quarter { get; set; }
        public string? CertificateSearch { get; set; }
        public string? Status { get; set; }
        public long? SellerId { get; set; }
    }
}
