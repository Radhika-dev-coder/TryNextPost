using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.SellerKYC
{
    public class BankAccountVerificationRequest
    {
        public string id_number { get; set; } = string.Empty;
        public string ifsc { get; set; } = string.Empty;
        public bool ifsc_details { get; set; } = true;
    }
}
