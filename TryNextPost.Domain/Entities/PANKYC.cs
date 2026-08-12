using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class PANKYC : BaseDbModel
    {
        [Key]
        public int Id { get; set; }
        public int? SellerKycId { get; set; }
        public SellerKYCDetails SellerKyc { get; set; }
        public string? PanNumber { get; set; }
        public string? MaskedAadhar { get; set; }
        public string? AadharVerified { get; set; }

        public string Name { get; set; }
        public DateTime DOB { get; set; }
        public string? Status { get; set; }
        public string? ProviderName { get; set; }
        public string? ClientId { get; set; }
        public string? ResponseCode { get; set; }
    }
}
