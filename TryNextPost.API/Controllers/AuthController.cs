using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TryNextPost.Application.DTO.Auth;
using TryNextPost.Application.Services.Interface;
using LoginRequest = TryNextPost.Application.DTO.Auth.LoginRequest;
using RegisterRequest = TryNextPost.Application.DTO.Auth.RegisterRequest;

namespace TryNextPost.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("check-email")]
        public async Task<IActionResult> CheckEmail(string email)
        {
            var exists = await _authService.CheckEmailAsync(email);
            return Ok(new { exists });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.LoginAsync(request, ip);
            return Ok(result);
        }


        [HttpPost("check-phone")]
        public async Task<IActionResult> CheckPhone([FromQuery] string mobile)
        {
            var exists = await _authService.CheckPhoneAsync(mobile);
            return Ok(new { exists });
        }

        [HttpPost("send-phone-otp")]
        public async Task<IActionResult> SendPhoneOtp([FromBody] SendPhoneOtpRequest request)
        {
            var message = await _authService.SendPhoneOtpAsync(request);
            return Ok(new { message });
        }

        [HttpPost("verify-phone-otp")]
        public async Task<IActionResult> VerifyPhoneOtp([FromBody] VerifyPhoneOtpRequest request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.VerifyPhoneOtpAsync(request, ip);
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.RegisterAsync(request, ip);
            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var result = await _authService.ForgotPasswordAsync(request);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var message = await _authService.ResetPasswordAsync(request);
            return Ok(new { message });
        }

        [HttpPost("verify-forgot-password-otp")]
        public async Task<IActionResult> VerifyForgotPasswordOtp([FromBody] VerifyForgotPasswordOtpRequest request)
        {
            var result = await _authService.VerifyForgotPasswordOtpAsync(request);
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.RefreshTokenAsync(request, ip ?? string.Empty);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest? request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated.");

            int? sessionId = null;
            var sidClaim = User.FindFirstValue("sid");
            if (int.TryParse(sidClaim, out var parsedSessionId))
                sessionId = parsedSessionId;

            var message = await _authService.LogoutAsync(userId, sessionId, request);
            return Ok(new { message });
        }
    }
}
