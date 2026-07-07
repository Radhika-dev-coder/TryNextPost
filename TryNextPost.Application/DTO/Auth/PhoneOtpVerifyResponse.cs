using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Auth
{
    public class PhoneOtpVerifyResponse
    {
        public bool IsRegistered { get; set; }
        public string Message { get; set; }
        public string Mobile {  get; set; }

        public string Token { get; set; } // Only when then IsRegisterd = True
        public DateTime? ExpiresAt { get;set; }
    }
}
