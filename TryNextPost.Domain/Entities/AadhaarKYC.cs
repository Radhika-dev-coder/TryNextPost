using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class AadhaarKYC : BaseDbModel
    {
        [Key]
        public int Id { get; set; }

        public int? SellerKycId { get; set; }
        public SellerKYCDetails SellerKyc {  get; set; }

        public string AadharLast4 { get; set; }
        public string VerficationStatus { get; set; }
        public string Name { get; set; }
        public DateTime DOB { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string? ProviderName { get; set; }
        public string? TransactionId { get; set; }
        public string? ResponseCode { get; set; }
    }
}
