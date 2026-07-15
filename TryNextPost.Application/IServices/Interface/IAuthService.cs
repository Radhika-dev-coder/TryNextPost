using Microsoft.AspNetCore.Identity.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Auth;
using TryNextPost.Application.DTO.Common;
using RegisterRequest = TryNextPost.Application.DTO.Auth.RegisterRequest;
using LoginRequest = TryNextPost.Application.DTO.Auth.LoginRequest;


namespace TryNextPost.Application.Services.Interface
{
    public interface IAuthService
    {
  
        //Email Path
        Task<bool> CheckEmailAsync(string email);
        Task<LoginSuccessResponse> LoginAsync(TryNextPost.Application.DTO.Auth.LoginRequest request,string ipAddress);

        //Phone Path
        Task<bool> CheckPhoneAsync(string mobile);
        Task<string> SendPhoneOtpAsync(SendPhoneOtpRequest request);
        Task<PhoneOtpVerifyResponse> VerifyPhoneOtpAsync(VerifyPhoneOtpRequest request, string ipAddress);

        //Call from Both Path A(Phone) and B(Email)
        Task<LoginSuccessResponse> RegisterAsync(RegisterRequest request, string ipAddress);

        Task<LoginOtpResponse> ForgotPasswordAsync(TryNextPost.Application.DTO.Auth.ForgotPasswordRequest request);
        Task<string> ResetPasswordAsync(TryNextPost.Application.DTO.Auth.ResetPasswordRequest request);

        Task<VerifyForgotPasswordOtpResponse> VerifyForgotPasswordOtpAsync(VerifyForgotPasswordOtpRequest request);


    }
}
