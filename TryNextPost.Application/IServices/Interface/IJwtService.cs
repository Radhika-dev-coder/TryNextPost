using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.IServices.Interface
{
    public interface IJwtService
    {
        string GenerateToken(string userId, string email,List<string> roles);
        string GenerateOtpToken(string email, string otp, DateTime expiry);
        (bool isValid, string email) ValidateOtpToken(string token, string enteredOtp);
        string GeneratePhoneVerifiedToken(string mobile);
    }
}
