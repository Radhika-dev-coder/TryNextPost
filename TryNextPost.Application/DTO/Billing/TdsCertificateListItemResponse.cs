namespace TryNextPost.Application.DTO.Billing
{
    public class TdsCertificateListItemResponse
    {
        public long TdsCertificateId { get; set; }
        public long SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public string CertificateNumber { get; set; } = string.Empty;
        public string FinancialYear { get; set; } = string.Empty;
        public string Quarter { get; set; } = string.Empty;
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public decimal Amount { get; set; }
        public string? DeductorName { get; set; }
        public string? DeductorTan { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string? OriginalFileName { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
