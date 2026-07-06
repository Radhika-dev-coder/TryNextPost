using Microsoft.AspNetCore.Identity.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Auth;
using TryNextPost.Application.DTO.Common;


namespace TryNextPost.Application.Services.Interface
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResult>> RegisterAsync(SellerDto dto);

        //Task<ApiResponse<AuthResult>> UpdateUser(UpdateSellerDto dto);

        //Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordDto dto);

        Task<bool> CheckEmailAsync(string email);
        Task<LoginOtpResponse> LoginAsync(TryNextPost.Application.DTO.Auth.LoginRequest request);
        Task<LoginSuccessResponse> VerifyOtpAsync(VerifyOtpRequest request, string ipAddress);

        Task<LoginOtpResponse> ResendOtpAsync(ResendOtpRequest request);

        Task<LoginOtpResponse> ForgotPasswordAsync(TryNextPost.Application.DTO.Auth.ForgotPasswordRequest request);
        Task<string> ResetPasswordAsync(TryNextPost.Application.DTO.Auth.ResetPasswordRequest request);
    }
}
