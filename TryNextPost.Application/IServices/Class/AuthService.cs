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
using RegisterRequest = TryNextPost.Application.DTO.Auth.RegisterRequest;
using LoginRequest = TryNextPost.Application.DTO.Auth.LoginRequest;


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
        private readonly ISmsService _smsService;

        public AuthService(IIdentityService identityService, ISellerRepository sellerRepository,IJwtService jwtService,IUserSessionRepository userSessionRepository,
            IEmailService emailService, IMemoryCache cache,ISmsService smsService)
        {
            _identityService = identityService;
            _sellerRepository = sellerRepository;
            _jwtService = jwtService;
            _emailService = emailService;
            _sessionRepository = userSessionRepository;
            _cache = cache;
            _smsService = smsService;

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

            var roles = await _identityService.GetUserRolesAsync(user.UserId);
            var token = _jwtService.GenerateToken(user.UserId, user.Email,roles);

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

        public async Task<LoginSuccessResponse> LoginAsync(LoginRequest request, string ipAddress)
        {
            var result = await _identityService.ValidateCredentialsAsync(request.Email, request.Password);

            if (!result.Succeeded)
                throw new UnauthorizedAccessException("Invalid email or password");

            var user = await _identityService.GetUserByEmailAsync(request.Email);
            var roles = await _identityService.GetUserRolesAsync(result.UserId);
            var token = _jwtService.GenerateToken(user.UserId, user.Email, roles);

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

            return new LoginSuccessResponse
            {
                Message = "Login successful",
                Token = token,
                ExpiresAt = session.ExpiryAt,
                Roles = roles
            };
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

        public async Task<LoginOtpResponse> SendOtpAsync(ResendOtpRequest request)
        {
            // Yaha CheckEmailExistsAsync CALL NAHI karna — email registered ho ya na ho, OTP bhejna hai dono case me
            var cacheKey = $"otp_resend_{request.Email}";
            if (_cache.TryGetValue(cacheKey, out DateTime lastSentTime))
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

            return new LoginOtpResponse { Message = "OTP sent to your email", OtpToken = otpToken };
        }

        public async Task<OtpVerifyResponse> VerifyOtpRequest(VerifyOtpRequest request, string ipAddress)
        {
            var (isValid, email) = _jwtService.ValidateOtpToken(request.OtpToken, request.Otp);

            if (!isValid)
                throw new UnauthorizedAccessException("Invalid or expired OTP");

            var isRegistered = await _identityService.CheckEmailExistsAsync(email);

            if (!isRegistered)
            {
                // Naya user — Dashboard nahi, Registration page bhejna hai
                return new OtpVerifyResponse
                {
                    IsRegistered = false,
                    Email = email,
                    Message = "Email verified. Please complete your registration."
                };
            }

            // Registered user — Login complete karo, JWT + Session banao
            var user = await _identityService.GetUserByEmailAsync(email);
            var roles = await _identityService.GetUserRolesAsync(user.UserId);
            var token = _jwtService.GenerateToken(user.UserId, user.Email, roles);

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

            return new OtpVerifyResponse
            {
                IsRegistered = true,
                Email = email,
                Message = "Login successful",
                Token = token,
                ExpiresAt = session.ExpiryAt
            };
        }

        public async Task<bool> CheckPhoneAsync(string mobile)
        {
           return await _identityService.CheckPhoneExistsAsyns(mobile);
        }

        public async Task<string> SendPhoneOtpAsync(SendPhoneOtpRequest request)
        {
            var cacheKey = $"phone_otp_{request.Mobile}";

            if (_cache.TryGetValue(cacheKey, out DateTime lastSentTime))
            {
                var secondsSinceLastSent = (DateTime.UtcNow - lastSentTime).TotalSeconds;
                if (secondsSinceLastSent < 30)
                {
                    var remaining = 30 - (int)secondsSinceLastSent;
                    throw new InvalidOperationException($"Please wait {remaining} seconds before requesting another OTP");
                }
            }

            var otp = new Random().Next(100000, 999999).ToString();
            await _smsService.SendOtpSms(request.Mobile, otp);

            _cache.Set(cacheKey, DateTime.UtcNow, TimeSpan.FromSeconds(30));

            return "OTP sent to your mobile number";
        }

        public async Task<PhoneOtpVerifyResponse> VerifyPhoneOtpAsync(VerifyPhoneOtpRequest request, string ipAddress)
        {
            if (string.IsNullOrEmpty(request.Otp) || request.Otp.Length != 6 || !request.Otp.All(char.IsDigit))
                throw new UnauthorizedAccessException("Invalid OTP format. Enter a 6-digit number.");

            var isRegistered = await _identityService.CheckPhoneExistsAsyns(request.Mobile);

            if(!isRegistered)
            {
                return new PhoneOtpVerifyResponse
                {
                    IsRegistered = false,
                    Mobile = request.Mobile,
                    Message = "Phone verified. Please complete your registration."
                };
            }

            // Registered → Then Complete Login
            var user = await _identityService.GetUserByPhoneAsync(request.Mobile);
            var roles = await _identityService.GetUserRolesAsync(user.UserId);
            var token = _jwtService.GenerateToken(user.UserId, user.Email,roles);

            var Session = new UserSession
            {
                UserId = user.UserId,
                DeviceId = request.DeviceId,
                IpAddress = ipAddress,
                JwtToken = token,
                CreatedAt = DateTime.UtcNow,
                ExpiryAt = DateTime.UtcNow.AddDays(7),
                IsActive = true
            };

            await _sessionRepository.AddAsync(Session);
            await _sessionRepository.SaveChangesAsync();

            return new PhoneOtpVerifyResponse
            {
                IsRegistered = true,
                Mobile = request.Mobile,
                Message = "Login successful",
                Token = token,
                ExpiresAt = Session.ExpiryAt
            };
        }

        public async Task<LoginSuccessResponse> RegisterAsync(RegisterRequest request, string ipAddress)
        {
            if (request.Password != request.ConfirmPassword)
                throw new InvalidOperationException("Password and Confirm Password do not match");

            var result = await _identityService.CreateUserAsync(
                request.Email, request.Password, request.FullName, request.Mobile);

            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors));

            await _emailService.SendWelcomeEmail(request.Email, request.FullName);

            var user = await _identityService.GetUserByPhoneAsync(request.Mobile);
            var roles = await _identityService.GetUserRolesAsync(user.UserId);
            var token = _jwtService.GenerateToken(result.UserId, request.Email,roles);

            var session = new UserSession
            {
                UserId = result.UserId,
                DeviceId = "registration-device",
                IpAddress = ipAddress,
                JwtToken = token,
                CreatedAt = DateTime.UtcNow,
                ExpiryAt = DateTime.UtcNow.AddDays(7),
                IsActive = true
            };

            await _sessionRepository.AddAsync(session);
            await _sessionRepository.SaveChangesAsync();

            return new LoginSuccessResponse
            {
                Message = "Registration successful! Welcome email sent.",
                Token = token,
                ExpiresAt = session.ExpiryAt
            };

        }
    }
}
