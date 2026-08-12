using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.SellerKYC
{
    public class PanComprehensiveRequest
    {
        public string id_number { get; set; } = string.Empty;

        public string masked_aadhaar_variant { get; set; } = "v1, v2, empty";
    }
}
