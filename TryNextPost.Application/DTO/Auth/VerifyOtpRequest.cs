using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Auth
{
    public class VerifyOtpRequest
    {
        public string OtpToken { get; set; }
        public string Otp { get; set; }
        public string DeviceId { get; set; }
    }
}
