using Microsoft.AspNetCore.Identity.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Auth;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.Services.Interface;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;
using Microsoft.Extensions.Caching.Memory;


namespace TryNextPost.Application.IServices.Class
{
    public class AuthService : IAuthService
    {
        private readonly IIdentityService _identityService;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IUserSessionRepository _sessionRepository;
        private readonly ISellerRepository _sellerRepository;
        private readonly IMemoryCache _cache;

        public AuthService(IIdentityService identityService, ISellerRepository sellerRepository,IJwtService jwtService,IUserSessionRepository userSessionRepository,
            IEmailService emailService, IMemoryCache cache)
        {
            _identityService = identityService;
            _sellerRepository = sellerRepository;
            _jwtService = jwtService;
            _emailService = emailService;
            _sessionRepository = userSessionRepository;
            _cache = cache;

        }


        public async Task<bool> CheckEmailAsync(string email)
        {
            return await _identityService.CheckEmailExistsAsync(email);
        }


        public async Task<LoginSuccessResponse> VerifyOtpAsync(VerifyOtpRequest request, string ipAddress)
        {
            var (isValid, email) = _jwtService.ValidateOtpToken(request.OtpToken, request.Otp);
            Console.WriteLine($"isValid: {isValid}, email: {email ?? "NULL"}");

            if (!isValid)
                throw new UnauthorizedAccessException("Invalid or expired OTP");

            var user = await _identityService.GetUserByEmailAsync(email);  
            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            var token = _jwtService.GenerateToken(user.UserId, user.Email);

            var session = new UserSession
            {
                UserId = user.UserId,
                DeviceId = request.DeviceId,
                IpAddress = ipAddress,
                JwtToken = token,
                CreatedAt = DateTime.UtcNow,
                ExpiryAt = DateTime.UtcNow.AddDays(7),
                IsActive = true
            };

            await _sessionRepository.AddAsync(session);
            await _sessionRepository.SaveChangesAsync();


            return new LoginSuccessResponse { Message = "Login successful", Token = token, ExpiresAt = session.ExpiryAt };
        }

        //public async Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordDto dto)
        //{
        //    var response = new ApiResponse<string>();

        //    var user = await _userManager.FindByIdAsync(dto.UserId);

        //    if (user == null)
        //    {
        //        response.Success = false;
        //        response.Message = "User not found";
        //        response.StatusCode = StatusCode.NotFound;
        //        return response;
        //    }

        //    if (dto.NewPassword != dto.ConfirmPassword)
        //    {
        //        response.Success = false;
        //        response.Message = "Password mismatch";
        //        response.StatusCode = StatusCode.BadRequest;
        //        return response;
        //    }

        //    var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

        //    if (!result.Succeeded)
        //    {
        //        response.Success = false;
        //        response.Message = "Password change failed";
        //        response.StatusCode = StatusCode.BadRequest;
        //        response.Errors = result.Errors.Select(e => e.Description).ToList();
        //        return response;
        //    }

        //    response.Success = true;
        //    response.Message = "Password changed successfully";
        //    response.StatusCode = StatusCode.Success;
        //    response.Data = "OK";

        //    return response;
        //}

        public async Task<ApiResponse<AuthResult>> RegisterAsync(SellerDto dto)
        {
            var identityResult = await _identityService.CreateUserAsync(dto.Email, dto.Password, dto.FullName, dto.PhoneNumber);


            if (!identityResult.Succeeded)
            {
                return new ApiResponse<AuthResult>
                {
                    Success = false,
                    Message = "Registration failed",
                    Errors = identityResult.Errors
                };
            }

            var seller = new Seller
            {
                UserId = identityResult.UserId,
                //CompanyId = dto.CompanyId,
                //GstNumber = dto.GstNumber,
                Status = SellerStatus.Active,
                CreatedAt = DateTime.UtcNow

            };

            await _sellerRepository.AddSellerAsync(seller);

            return new ApiResponse<AuthResult>
            {
                Success = true,
                Message = "User registered successfully",
                Data = new AuthResult
                {
                    UserId = identityResult.UserId,
                    Email = dto.Email
                }
            };
        }

        public async Task<LoginOtpResponse> LoginAsync(DTO.Auth.LoginRequest request)
        {
            try
            {
                var result = await _identityService.ValidateCredentialsAsync(request.Email, request.Password);
                Console.WriteLine("Step 1 done"); // debug marker

                if (!result.Succeeded)
                    throw new UnauthorizedAccessException("Invalid email or password");

                var otp = new Random().Next(100000, 999999).ToString();
                Console.WriteLine("Step 2 done");

                var otpToken = _jwtService.GenerateOtpToken(request.Email, otp, DateTime.UtcNow.AddMinutes(5));
                Console.WriteLine("Step 3 done");

                await _emailService.SendOtpEmail(request.Email, otp);
                Console.WriteLine("Step 4 done");

                return new LoginOtpResponse { Message = "OTP sent", OtpToken = otpToken };
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.ToString());   
                throw;
            }
        }

        public async Task<LoginOtpResponse> ResendOtpAsync(ResendOtpRequest request)
        {
            
            var emailExists = await _identityService.CheckEmailExistsAsync(request.Email);

            if (!emailExists)
                throw new UnauthorizedAccessException("Email not found");

            var cacheKey = $"otp_resend_{request.Email}";
            if (_cache.TryGetValue(cacheKey, out DateTime lastSentTime))
            {
                var secondsSinceLastSent = (DateTime.UtcNow - lastSentTime).TotalSeconds;
                var waitTime = 30;

                if (secondsSinceLastSent < waitTime)
                {
                    var remainingSeconds = waitTime - (int)secondsSinceLastSent;
                    throw new InvalidOperationException($"Please wait {remainingSeconds} seconds before requesting another OTP");
                }
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var otpToken = _jwtService.GenerateOtpToken(request.Email, otp, DateTime.UtcNow.AddMinutes(5));

            await _emailService.SendOtpEmail(request.Email, otp);

            _cache.Set(cacheKey, DateTime.UtcNow, TimeSpan.FromSeconds(30));

            return new LoginOtpResponse
            {
                Message = "OTP resent to your email",
                OtpToken = otpToken
            };
        }


        public async Task<LoginOtpResponse> ForgotPasswordAsync(DTO.Auth.ForgotPasswordRequest request)
        {
            var emailExits = await _identityService.CheckEmailExistsAsync(request.Email);
            if (!emailExits) throw new UnauthorizedAccessException("Email Not Found");

            var cacheKey = $"otp_resend_{request.Email}";
            if(_cache.TryGetValue(cacheKey,out DateTime lastSentTime))
            {
                var secondsSinceLastSent = (DateTime.UtcNow - lastSentTime).TotalSeconds;
                if (secondsSinceLastSent < 30)
                {
                    var remaining = 30 - (int)secondsSinceLastSent;
                    throw new InvalidOperationException($"Please wait {remaining} seconds before requesting another OTP");
                }
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var otpToken = _jwtService.GenerateOtpToken(request.Email, otp, DateTime.UtcNow.AddMinutes(5));

            await _emailService.SendOtpEmail(request.Email, otp);

            _cache.Set(cacheKey, DateTime.UtcNow, TimeSpan.FromSeconds(30));

            return new LoginOtpResponse
            {
                Message = "OTP sent to your email for password reset",
                OtpToken = otpToken
            };
        }

        public async Task<string> ResetPasswordAsync(DTO.Auth.ResetPasswordRequest request)
        {
            if (request.NewPassword != request.ConfirmPassword)
                throw new InvalidOperationException("Passwords do not match");

            var (isValid, email) = _jwtService.ValidateOtpToken(request.OtpToken, request.Otp);

            if (!isValid)
                throw new UnauthorizedAccessException("Invalid or expired OTP");

            var result = await _identityService.ResetPasswordAsync(email, request.NewPassword);

            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors));

            return "Password reset successful. Please login with your new password.";
        }
    }
}
