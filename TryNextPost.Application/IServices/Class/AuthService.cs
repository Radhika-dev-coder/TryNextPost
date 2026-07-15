    using Microsoft.AspNetCore.Identity.Data;
    using Microsoft.Extensions.Caching.Memory;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TryNextPost.Application.DTO.Auth;
    using TryNextPost.Application.DTO.Common;
    using TryNextPost.Application.IServices.Interface;
    using TryNextPost.Application.Services.Interface;
using TryNextPost.Domain.Common;
    using TryNextPost.Domain.Entities;
    using TryNextPost.Domain.Enums;
    using TryNextPost.Domain.IRepository;
    using LoginRequest = TryNextPost.Application.DTO.Auth.LoginRequest;
    using RegisterRequest = TryNextPost.Application.DTO.Auth.RegisterRequest;


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
            private readonly IUnitOfWork _unitOfWork;

            public AuthService(IIdentityService identityService, ISellerRepository sellerRepository,IJwtService jwtService,IUserSessionRepository userSessionRepository,
                IEmailService emailService, IMemoryCache cache,ISmsService smsService,IUnitOfWork unitOfWork)
            {
                _identityService = identityService;
                _sellerRepository = sellerRepository;
                _jwtService = jwtService;
                _emailService = emailService;
                _sessionRepository = userSessionRepository;
                _cache = cache;
                _smsService = smsService;
                _unitOfWork = unitOfWork;

            }


            public async Task<bool> CheckEmailAsync(string email)
            {
                return await _identityService.CheckEmailExistsAsync(email);
            }


            public async Task<LoginSuccessResponse> LoginAsync(LoginRequest request, string ipAddress)
            {
                var result = await _identityService.ValidateCredentialsAsync(request.Email, request.Password);

                if (!result.Succeeded)
                    throw new UnauthorizedAccessException(SystemMessage.InvalidCredentials);

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
                    Message = SystemMessage.LoginSuccess,
                    Token = token,
                    ExpiresAt = session.ExpiryAt,
                    Roles = roles
                };
            }



            public async Task<LoginOtpResponse> ForgotPasswordAsync(DTO.Auth.ForgotPasswordRequest request)
            {
                var emailExits = await _identityService.CheckEmailExistsAsync(request.Email);
                if (!emailExits) throw new UnauthorizedAccessException(string.Format(SystemMessage.EmailNotFound));

                var cacheKey = $"otp_resend_{request.Email}";
                if(_cache.TryGetValue(cacheKey,out DateTime lastSentTime))
                {
                    var secondsSinceLastSent = (DateTime.UtcNow - lastSentTime).TotalSeconds;
                    if (secondsSinceLastSent < 30)
                    {
                        var remaining = 30 - (int)secondsSinceLastSent;
                        throw new InvalidOperationException(string.Format(SystemMessage.OtpWaitMessage,remaining));
                    }
                }

                var otp = new Random().Next(100000, 999999).ToString();
                var otpToken = _jwtService.GenerateOtpToken(request.Email, otp, DateTime.UtcNow.AddMinutes(5));

                await _emailService.SendOtpEmail(request.Email, otp);

                _cache.Set(cacheKey, DateTime.UtcNow, TimeSpan.FromSeconds(30));

                return new LoginOtpResponse
                {
                    Message = SystemMessage.OtpSentEmail,
                    OtpToken = otpToken
                };
            }

            public async Task<string> ResetPasswordAsync(DTO.Auth.ResetPasswordRequest request)
            {
            //if (request.NewPassword != request.ConfirmPassword)
            //    throw new InvalidOperationException(string.Format(SystemMessage.PasswordMismatch));

            //var (isValid, email) = _jwtService.ValidateOtpToken(request.OtpToken, request.Otp);

            //if (!isValid)
            //    throw new UnauthorizedAccessException(string.Format(SystemMessage.InvalidOtp));

            //var result = await _identityService.ResetPasswordAsync(email, request.NewPassword);

            //if (!result.Succeeded)
            //    throw new InvalidOperationException(string.Join(", ", result.Errors));

            //return SystemMessage.PasswordResetSuccess;

            var (isValid, email) = _jwtService.ValidateOtpToken(request.ResetToken, "VERIFIED");

            if (!isValid)
                throw new UnauthorizedAccessException("Invalid or expired reset session. Please try again.");

            var result = await _identityService.ResetPasswordAsync(email, request.NewPassword);

            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors));

            return SystemMessage.PasswordResetSuccess;
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
                        throw new InvalidOperationException(string.Format(SystemMessage.OtpWaitMessage,remaining));
                    }
                }

                var otp = new Random().Next(100000, 999999).ToString();
                await _smsService.SendOtpSms(request.Mobile, otp);

                _cache.Set(cacheKey, DateTime.UtcNow, TimeSpan.FromSeconds(30));

                return SystemMessage.OtpSentPhone;
            }

            public async Task<PhoneOtpVerifyResponse> VerifyPhoneOtpAsync(VerifyPhoneOtpRequest request, string ipAddress)
            {
                if (string.IsNullOrEmpty(request.Otp) || request.Otp.Length != 6 || !request.Otp.All(char.IsDigit))
                    throw new UnauthorizedAccessException(string.Format(SystemMessage.InvalidOtpFormat));

                var isRegistered = await _identityService.CheckPhoneExistsAsyns(request.Mobile);

                if(!isRegistered)
                {
                    return new PhoneOtpVerifyResponse
                    {
                        IsRegistered = false,
                        Mobile = request.Mobile,
                        Message = SystemMessage.PhoneVerifiedRegistrationRequired
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
                 await _unitOfWork.BeginTransactionAsync();

               try
            {
                //if (request.Password != request.ConfirmPassword)
                //    throw new InvalidOperationException("Password and Confirm Password do not match");

                var fullName = $"{request.FirstName} {request.LastName}".Trim();

                var result = await _identityService.CreateUserAsync(
                    request.Email, request.Password, fullName, request.Mobile);

                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join(", ", result.Errors));

                var roles = await _identityService.GetUserRolesAsync(result.UserId);
                var token = _jwtService.GenerateToken(result.UserId, request.Email, roles);
                await _sellerRepository.CreateSellerAsync(result.UserId);

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
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                await _emailService.SendWelcomeEmail(request.Email, fullName);

                return new LoginSuccessResponse
                {
                    Message = SystemMessage.RegisterSuccess,
                    Token = token,
                    ExpiresAt = session.ExpiryAt,
                    Roles = roles
                };
            }

            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            }

        public async Task<VerifyForgotPasswordOtpResponse> VerifyForgotPasswordOtpAsync(VerifyForgotPasswordOtpRequest request)
        {
            var (isValid, email) = _jwtService.ValidateOtpToken(request.OtpToken, request.Otp);

            if (!isValid)
                throw new UnauthorizedAccessException(SystemMessage.InvalidOtp);
            var resetToken = _jwtService.GenerateOtpToken(email, "VERIFIED", DateTime.UtcNow.AddMinutes(10));

            return new VerifyForgotPasswordOtpResponse
            {
                Message =SystemMessage.VerifiedOtp,
                ResetToken = resetToken
            };
        }
    }
    }
