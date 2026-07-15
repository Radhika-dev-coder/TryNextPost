using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Auth
{
    public class VerifyForgotPasswordOtpResponse
    {
        public string Message { get; set; } = string.Empty;
        public string ResetToken { get; set; } = string.Empty;
    }
}
