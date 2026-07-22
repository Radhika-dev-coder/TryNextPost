using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using TryNextPost.Application.DTO.Auth;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.Services.Interface;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.IRepository;
using LoginRequest = TryNextPost.Application.DTO.Auth.LoginRequest;
using RegisterRequest = TryNextPost.Application.DTO.Auth.RegisterRequest;

namespace TryNextPost.Application.IServices.Class
{
    public class AuthService : IAuthService
    {
        private const int MaxEmailOtpAttempts = 5;
        private const int RefreshTokenDays = 30;

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

        public AuthService(
            IIdentityService identityService,
            ISellerRepository sellerRepository,
            IJwtService jwtService,
            IUserSessionRepository userSessionRepository,
            IEmailService emailService,
            IMemoryCache cache,
            ISmsService smsService,
            IUnitOfWork unitOfWork,
            IOtpRepository otpRepository,
            IConfiguration configuration,
            ISellerContextService sellerContextService)
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

        public Task<bool> CheckEmailAsync(string email)
            => _identityService.CheckEmailExistsAsync(email);

        public async Task<LoginSuccessResponse> LoginAsync(LoginRequest request, string ipAddress)
        {
            var result = await _identityService.ValidateCredentialsAsync(request.Email, request.Password);
            if (!result.Succeeded)
                throw new UnauthorizedAccessException(SystemMessage.InvalidCredentials);

            var user = await _identityService.GetUserByEmailAsync(request.Email);
            var roles = await _identityService.GetUserRolesAsync(result.UserId);

            return await BuildLoginResponseAsync(
                user.UserId,
                user.Email,
                roles,
                request.DeviceId,
                ipAddress,
                SystemMessage.LoginSuccess);
        }

        public async Task<LoginOtpResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var emailExists = await _identityService.CheckEmailExistsAsync(request.Email);
            if (!emailExists)
                throw new UnauthorizedAccessException(string.Format(SystemMessage.EmailNotFound));

            var cacheKey = $"otp_resend_{request.Email}";
            if (_cache.TryGetValue(cacheKey, out DateTime lastSentTime))
            {
                var secondsSinceLastSent = (DateTime.UtcNow - lastSentTime).TotalSeconds;
                if (secondsSinceLastSent < 60)
                {
                    var remaining = 60 - (int)secondsSinceLastSent;
                    throw new InvalidOperationException(string.Format(SystemMessage.OtpWaitMessage, remaining));
                }
            }

            _cache.Remove($"email_otp_attempts_{request.Email}");

            var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var otpToken = _jwtService.GenerateOtpToken(request.Email, otp, DateTime.UtcNow.AddMinutes(5));

            await _emailService.SendOtpEmail(request.Email, otp);
            _cache.Set(cacheKey, DateTime.UtcNow, TimeSpan.FromSeconds(60));

            return new LoginOtpResponse
            {
                Message = SystemMessage.OtpSentEmail,
                OtpToken = otpToken
            };
        }

        public async Task<string> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var (isValid, email) = _jwtService.ValidateOtpToken(request.ResetToken, "VERIFIED");
            if (!isValid)
                throw new UnauthorizedAccessException("Invalid or expired reset session. Please try again.");

            var result = await _identityService.ResetPasswordAsync(email, request.NewPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors));

            return SystemMessage.PasswordResetSuccess;
        }

        public Task<bool> CheckPhoneAsync(string mobile)
            => _identityService.CheckPhoneExistsAsyns(mobile);

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

            var isRegistered = await _identityService.CheckPhoneExistsAsyns(mobile);
            if (isRegistered)
            {
                var user = await _identityService.GetUserByPhoneAsync(mobile);
                var roles = await _identityService.GetUserRolesAsync(user.UserId);
                var login = await BuildLoginResponseAsync(
                    user.UserId,
                    user.Email,
                    roles,
                    "phone-otp-login",
                    ipAddress,
                    SystemMessage.LoginSuccess);

                return new PhoneOtpVerifyResponse
                {
                    IsRegistered = true,
                    Token = login.Token,
                    ExpiresAt = login.ExpiresAt,
                    Message = SystemMessage.LoginSuccess
                };
            }

            var phoneVerifiedToken = _jwtService.GeneratePhoneVerifiedToken(mobile);
            return new PhoneOtpVerifyResponse
            {
                IsRegistered = false,
                Mobile = request.Mobile,
                PhoneVerifiedToken = phoneVerifiedToken,
                Message = SystemMessage.PhoneVerifiedRegistrationRequired
            };
        }

        public async Task<LoginSuccessResponse> RegisterAsync(RegisterRequest request, string ipAddress)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var fullName = $"{request.FirstName} {request.LastName}".Trim();

                var result = await _identityService.CreateUserAsync(
                    request.Email, request.Password, fullName, request.Mobile);

                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join(", ", result.Errors));

                var roles = await _identityService.GetUserRolesAsync(result.UserId);
                await _sellerRepository.CreateSellerAsync(result.UserId);

                var loginResponse = await BuildLoginResponseAsync(
                    result.UserId,
                    request.Email,
                    roles,
                    "registration-device",
                    ipAddress,
                    SystemMessage.RegisterSuccess);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                await _emailService.SendWelcomeEmail(request.Email, fullName);

                var sellerContext = new SellerContextDto
                {
                    SellerId = (await _sellerRepository.GetByUserIdAsync(result.UserId)).SellerId,
                    IsOwner = true,
                    Permissions = Domain.Enums.EmployeePermissionCode.All.ToList()
                };

                loginResponse.SellerContext = sellerContext;
                return loginResponse;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<VerifyForgotPasswordOtpResponse> VerifyForgotPasswordOtpAsync(VerifyForgotPasswordOtpRequest request)
        {
            var attemptKey = GetEmailOtpAttemptCacheKey(request.OtpToken);
            var attempts = _cache.GetOrCreate(attemptKey, _ => 0);
            if (attempts >= MaxEmailOtpAttempts)
                throw new InvalidOperationException(SystemMessage.RequestNewOtp);

            var (isValid, email) = _jwtService.ValidateOtpToken(request.OtpToken, request.Otp);
            if (!isValid)
            {
                _cache.Set(attemptKey, attempts + 1, TimeSpan.FromMinutes(10));
                throw new UnauthorizedAccessException(SystemMessage.InvalidOtp);
            }

            _cache.Remove(attemptKey);
            var resetToken = _jwtService.GenerateOtpToken(email, "VERIFIED", DateTime.UtcNow.AddMinutes(10));

            return new VerifyForgotPasswordOtpResponse
            {
                Message = SystemMessage.VerifiedOtp,
                ResetToken = resetToken
            };
        }

        public async Task<LoginSuccessResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new UnauthorizedAccessException(SystemMessage.InvalidRefreshToken);

            var refreshHash = _jwtService.HashRefreshToken(request.RefreshToken);
            var session = await _sessionRepository.GetByRefreshTokenHashAsync(refreshHash)
                ?? throw new UnauthorizedAccessException(SystemMessage.InvalidRefreshToken);

            if (!session.IsActive
                || session.RefreshTokenExpiryAt == null
                || session.RefreshTokenExpiryAt < DateTime.UtcNow)
            {
                session.IsActive = false;
                await _sessionRepository.SaveChangesAsync();
                throw new UnauthorizedAccessException(SystemMessage.InvalidRefreshToken);
            }

            var user = await _identityService.GetUserByIdAsync(session.UserId)
                ?? throw new UnauthorizedAccessException(SystemMessage.UserNotFound);

            var roles = await _identityService.GetUserRolesAsync(session.UserId);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var refreshExpiry = DateTime.UtcNow.AddDays(RefreshTokenDays);

            session.JwtToken = _jwtService.GenerateToken(user.UserId, user.Email, roles, session.Id);
            session.RefreshTokenHash = _jwtService.HashRefreshToken(newRefreshToken);
            session.RefreshTokenExpiryAt = refreshExpiry;
            session.ExpiryAt = DateTime.UtcNow.AddDays(7);
            session.IpAddress = ipAddress;
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
            }

            return new LoginSuccessResponse
            {
                Message = SystemMessage.LoginSuccess,
                Token = session.JwtToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = session.ExpiryAt,
                RefreshTokenExpiryTime = refreshExpiry,
                Roles = roles,
                SellerContext = sellerContext
            };
        }

        public async Task<string> LogoutAsync(string userId, int? sessionId, LogoutRequest? request)
        {
            UserSession? session = null;

            if (sessionId.HasValue)
                session = await _sessionRepository.GetByIdAsync(sessionId.Value);

            if (session == null && !string.IsNullOrWhiteSpace(request?.RefreshToken))
            {
                var refreshHash = _jwtService.HashRefreshToken(request.RefreshToken);
                session = await _sessionRepository.GetByRefreshTokenHashAsync(refreshHash);
            }

            if (session == null || session.UserId != userId)
                throw new UnauthorizedAccessException(SystemMessage.SessionRevoked);

            session.IsActive = false;
            session.RefreshTokenHash = null;
            session.RefreshTokenExpiryAt = null;
            await _sessionRepository.SaveChangesAsync();

            return SystemMessage.LogoutSuccess;
        }

        private async Task<LoginSuccessResponse> BuildLoginResponseAsync(
            string userId,
            string email,
            List<string> roles,
            string deviceId,
            string ipAddress,
            string message)
        {
            var refreshToken = _jwtService.GenerateRefreshToken();
            var refreshExpiry = DateTime.UtcNow.AddDays(RefreshTokenDays);

            var session = new UserSession
            {
                UserId = userId,
                DeviceId = deviceId,
                IpAddress = ipAddress,
                JwtToken = string.Empty,
                CreatedAt = DateTime.UtcNow,
                ExpiryAt = DateTime.UtcNow.AddDays(7),
                RefreshTokenHash = _jwtService.HashRefreshToken(refreshToken),
                RefreshTokenExpiryAt = refreshExpiry,
                IsActive = true
            };

            await _sessionRepository.AddAsync(session);
            await _sessionRepository.SaveChangesAsync();

            session.JwtToken = _jwtService.GenerateToken(userId, email, roles, session.Id);
            await _sessionRepository.SaveChangesAsync();

            SellerContextDto? sellerContext = null;
            try
            {
                var context = await _sellerContextService.ResolveAsync(userId);
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
            }

            return new LoginSuccessResponse
            {
                Message = message,
                Token = session.JwtToken,
                RefreshToken = refreshToken,
                ExpiresAt = session.ExpiryAt,
                RefreshTokenExpiryTime = refreshExpiry,
                Roles = roles,
                SellerContext = sellerContext
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

        private static string GetEmailOtpAttemptCacheKey(string otpToken)
        {
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(otpToken)));
            return $"email_otp_attempts_{hash}";
        }
    }
}
