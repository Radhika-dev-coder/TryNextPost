using Microsoft.AspNetCore.Http;

namespace TryNextPost.Application.DTO.Billing
{
    public class TdsCertificateUploadRequest
    {
        public long SellerId { get; set; }
        public string FinancialYear { get; set; } = string.Empty;
        public string Quarter { get; set; } = string.Empty;
        public string CertificateNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public IFormFile File { get; set; } = null!;
        public string? DeductorName { get; set; }
        public string? DeductorTan { get; set; }
        public string? Remarks { get; set; }
    }
}