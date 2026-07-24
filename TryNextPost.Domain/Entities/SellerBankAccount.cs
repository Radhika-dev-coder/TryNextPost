using System.ComponentModel.DataAnnotations;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class SellerBankAccount : BaseDbModel
    {
        [Key]
        public long SellerBankAccountId { get; set; }

        public long SellerId { get; set; }
        public Seller? Seller { get; set; }

        [MaxLength(150)]
        public string AccountHolderName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        [MaxLength(20)]
        public string IfscCode { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? BankName { get; set; }

        [MaxLength(100)]
        public string? BranchName { get; set; }

        [MaxLength(30)]
        public string? AccountType { get; set; }

        public bool IsPrimary { get; set; }
    }
}
