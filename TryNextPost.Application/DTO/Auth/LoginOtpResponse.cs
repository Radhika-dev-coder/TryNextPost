using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Auth
{
    public class LoginOtpResponse
    {
        public string Message { get; set; }
        public string OtpToken { get; set; }
    }
}
