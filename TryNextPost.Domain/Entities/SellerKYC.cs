using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class SellerKYC : BaseDbModel
    {
        [Key]
        public int Id { get; set; }

        public string SellerId { get; set; }
        public string? PanNumber { get; set; }
        public string? PanHolderName { get; set; }
        public string? PanVerfied { get; set; }
        public DateTime? PanVerfiedOn { get; set; }
        public string? AadharLast4Digit { get; set; }
        public string? AadharReferenceId { get; set; }
        public string? AadharVerified {  get; set; }
        public DateTime? AadharVerifiedOn { get; set; }

        public string KYCStatus { get; set; }
         public string? VerificationProvider { get; set; }
        public string? VerificationReferenceId { get; set; }
        public int? FailureCode { get; set; }
        public string? FailureReason { get; set; }
        public int? RetryCount { get; set; } = 0;
       public string? VerficationBy { get; set; }
        public string? Remark { get;  set; }

        
    }
}
