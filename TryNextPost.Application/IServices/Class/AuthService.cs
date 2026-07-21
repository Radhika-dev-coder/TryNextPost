using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.Data;
    using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
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
            private readonly ISellerContextService _sellerContextService;
            private readonly IMemoryCache _cache;
            private readonly ISmsService _smsService;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IOtpRepository _otpRepository;
            private readonly IConfiguration _configuration;

        public AuthService(IIdentityService identityService, ISellerRepository sellerRepository,IJwtService jwtService,IUserSessionRepository userSessionRepository,
                IEmailService emailService, IMemoryCache cache,ISmsService smsService,IUnitOfWork unitOfWork,
                IOtpRepository otpRepository,IConfiguration configuration, ISellerContextService sellerContextService)
            {
                _identityService = identityService;
                _sellerRepository = sellerRepository;
                _jwtService = jwtService;
                _emailService = emailService;
                _sessionRepository = userSessionRepository;
                _cache = cache;
                _smsService = smsService;
                _unitOfWork = unitOfWork;
            _otpRepository = otpRepository;
            _configuration = configuration;
            _sellerContextService = sellerContextService;

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

                SellerContextDto? sellerContext = null;
                try
                {
                    var context = await _sellerContextService.ResolveAsync(user.UserId);
                    sellerContext = new SellerContextDto
                    {
                        SellerId = context.SellerId,
                        IsOwner = context.IsOwner,
                        EmployeeId = context.EmployeeId,
                        Permissions = context.Permissions.ToList()
                    };
                }
                catch (UnauthorizedAccessException)
                {
                    // SuperAdmin / non-seller logins have no seller context.
                }

                return new LoginSuccessResponse
                {
                    Message = SystemMessage.LoginSuccess,
                    Token = token,
                    ExpiresAt = session.ExpiryAt,
                    Roles = roles,
                    SellerContext = sellerContext
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
                    if (secondsSinceLastSent < 60)
                    {
                        var remaining = 60 - (int)secondsSinceLastSent;
                        throw new InvalidOperationException(string.Format(SystemMessage.OtpWaitMessage,remaining));
                    }
                }

                var otp = new Random().Next(100000, 999999).ToString();
                var otpToken = _jwtService.GenerateOtpToken(request.Email, otp, DateTime.UtcNow.AddMinutes(5));

                await _emailService.SendOtpEmail(request.Email, otp);

                _cache.Set(cacheKey, DateTime.UtcNow, TimeSpan.FromSeconds(60));

                return new LoginOtpResponse
                {
                    Message = SystemMessage.OtpSentEmail,
                    OtpToken = otpToken
                };
            }

            public async Task<string> ResetPasswordAsync(DTO.Auth.ResetPasswordRequest request)
            {
  
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
            var mobile = NormalizedIndianMobile(request.Mobile);

            var cacheKey = $"phone_otp_{mobile}";
            if (_cache.TryGetValue(cacheKey, out _))
                throw new InvalidOperationException("Please wait before requesting another OTP");

            await _otpRepository.InvalidateActiveOtpsAsync(mobile);

            var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var entity = new Otp
            {
                MobileNumber = mobile,
                CodeHash = HashOtp(otp, mobile),
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                FailedAttempts = 0
            };

            await _smsService.SendOtpSms(mobile, otp);
            await _otpRepository.AddAsync(entity);
            await _otpRepository.SaveChangesAsync();
            _cache.Set(cacheKey, true, TimeSpan.FromSeconds(60));
            return SystemMessage.OtpSentPhone;
        }

            public async Task<PhoneOtpVerifyResponse> VerifyPhoneOtpAsync(VerifyPhoneOtpRequest request, string ipAddress)
            {

            var mobile = NormalizedIndianMobile(request.Mobile);

            if (string.IsNullOrEmpty(request.Otp) || request.Otp.Length != 6 || !request.Otp.All(char.IsDigit))
                throw new UnauthorizedAccessException(SystemMessage.InvalidOtpFormat);

           
            var otpEntity = await _otpRepository.GetLatestActiveByMobileAsync(mobile);

            if (otpEntity == null || otpEntity.ExpiryTime < DateTime.UtcNow)
                throw new UnauthorizedAccessException(SystemMessage.InvalidOtp);

            if (otpEntity.ExpiryTime < DateTime.UtcNow)
                throw new UnauthorizedAccessException(SystemMessage.OtpExpired);

            if (otpEntity.FailedAttempts >= 5)
                throw new InvalidOperationException(SystemMessage.RequestNewOtp);


            var incomingHash = HashOtp(request.Otp, mobile);

            if (!CryptographicOperations.FixedTimeEquals(
                    Convert.FromHexString(otpEntity.CodeHash),
                    Convert.FromHexString(incomingHash)))
            {
                otpEntity.FailedAttempts++;
                await _otpRepository.SaveChangesAsync();
                throw new InvalidOperationException(SystemMessage.InvalidOtp);
            }

            otpEntity.IsUsed = true;
            await _otpRepository.SaveChangesAsync();

            var user = await _identityService.CheckPhoneExistsAsyns(mobile);

            if (user != null)
            {
                return new PhoneOtpVerifyResponse
                {
                    IsRegistered = true,
                };
            }

            var isRegistered = await _identityService.CheckPhoneExistsAsyns(mobile);

            if (!isRegistered)
            {
                return new PhoneOtpVerifyResponse
                {
                    IsRegistered = false,
                    Mobile = request.Mobile,
                    Message = SystemMessage.PhoneVerifiedRegistrationRequired
                };
            }

            var phoneVerifiedToken = _jwtService.GeneratePhoneVerifiedToken(mobile);
            return new PhoneOtpVerifyResponse
            {
                IsRegistered = false,
                PhoneVerifiedToken = phoneVerifiedToken,
                Message = "Complete registration"
            };
        }



        private static string NormalizedIndianMobile(string mobile)
        {
            mobile = mobile.Trim().Replace(" ", "").Replace("-", "");
            if (mobile.StartsWith("+91"))
                mobile = mobile[3..];
            else if (mobile.StartsWith("91") && mobile.Length == 12)
                mobile = mobile[2..];
            else if (mobile.StartsWith("0") && mobile.Length == 11)
                mobile = mobile[1..];
            if (mobile.Length != 10 || mobile[0] < '6')
                throw new InvalidOperationException(SystemMessage.InvalidMobile);
            return "91" + mobile;
        }

        private string HashOtp(string otp, string mobile)
        {
            var pepper = _configuration["Otp:Pepper"]
             ?? throw new InvalidOperationException("Otp:Pepper missing");
            var bytes = Encoding.UTF8.GetBytes($"{otp}:{mobile}:{pepper}");
            return Convert.ToHexString(SHA256.HashData(bytes));
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

                var sellerContext = new SellerContextDto
                {
                    SellerId = (await _sellerRepository.GetByUserIdAsync(result.UserId)).SellerId,
                    IsOwner = true,
                    Permissions = EmployeePermissionCode.All.ToList()
                };

                return new LoginSuccessResponse
                {
                    Message = SystemMessage.RegisterSuccess,
                    Token = token,
                    ExpiresAt = session.ExpiryAt,
                    Roles = roles,
                    SellerContext = sellerContext
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
