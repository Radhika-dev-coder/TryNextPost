using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Auth
{
    public class OtpVerifyResponse
    {
        public bool IsRegistered { get; set; }
        public string Message { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }          // only then generated if  IsRegistered = true
        public DateTime? ExpiresAt { get; set; }
    }
}
