using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities
{
    public class TdsCertificate : BaseDbModel
    {
        [Key]
        public long TdsCertificateId { get; set; }

        public long SellerId { get; set; }
        public Seller? Seller { get; set; }

        [MaxLength(20)]
        public string FinancialYear { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Quarter { get; set; } = string.Empty;

        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }

        [MaxLength(100)]
        public string CertificateNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(200)]
        public string? DeductorName { get; set; }

        [MaxLength(20)]
        public string? DeductorTan { get; set; }

        [MaxLength(500)]
        public string FileUrl { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? OriginalFileName { get; set; }

        public TdsCertificateStatus Status { get; set; } = TdsCertificateStatus.Issued;

        [MaxLength(500)]
        public string? Remarks { get; set; }
    }
}
