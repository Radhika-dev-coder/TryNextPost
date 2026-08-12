using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class BankKYC : BaseDbModel
    {
        [Key]
        public int Id { get; set; }
        public int? SellerKycId { get; set; }
        public SellerKYCDetails SellerKyc { get; set; }
        public string? AccountHolderName { get; set; }
        public string? AccountNumberMasked { get; set; }
        public string? IFSC {  get; set; }
        public string? BankName { get; set; } 
        public string? BranchName { get; set; }
        public string? AccountStatus { get; set; }
        public string? Status { get; set; }
        public string? ProviderName { get; set; }
        public string? ClientId { get; set; }
        public string? ResponseCode { get; set; }
    }
}
