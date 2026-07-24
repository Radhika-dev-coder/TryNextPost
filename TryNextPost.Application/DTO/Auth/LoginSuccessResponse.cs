using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Auth
{
    public class LoginSuccessResponse
    {
        public string Message { get; set; }
        public string Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public List<string> Roles { get; set; }

        public SellerContextDto? SellerContext { get; set; }

        /// <summary>
        /// True when Seller/Admin/SellerEmployee logged in but has no seller profile yet.
        /// Frontend should show a KYC / complete-profile alert. Tokens are still issued.
        /// SuperAdmin always false (seller context N/A).
        /// </summary>
        public bool RequiresKyc { get; set; }

        /// <summary>
        /// False when RequiresKyc is true; true when seller context resolved;
        /// null for SuperAdmin (profile completeness N/A).
        /// </summary>
        public bool? IsProfileComplete { get; set; }
    }
}
