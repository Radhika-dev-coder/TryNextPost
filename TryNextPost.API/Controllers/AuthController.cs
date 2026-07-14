using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using TryNextPost.Application.DTO;
using TryNextPost.Application.DTO.Auth;
using TryNextPost.Application.IServices;
using TryNextPost.Application.Services.Interface;
using RegisterRequest = TryNextPost.Application.DTO.Auth.RegisterRequest;
using LoginRequest = TryNextPost.Application.DTO.Auth.LoginRequest;

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
        public async Task<IActionResult> Login([FromBody]LoginRequest request)
        {

                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var result = await _authService.LoginAsync(request,ip);
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
        public async Task<IActionResult> ForgotPassword([FromBody] TryNextPost.Application.DTO.Auth.ForgotPasswordRequest request)
        {

                var result = await _authService.ForgotPasswordAsync(request);
                return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] TryNextPost.Application.DTO.Auth.ResetPasswordRequest request)
        {

                var message = await _authService.ResetPasswordAsync(request);
                return Ok(new { message });
        }

        //[HttpPost("send-otp")]
        //public async Task<IActionResult> SendOtp([FromBody] ResendOtpRequest request)
        //{
        //    try
        //    {
        //        var result = await _authService.SendOtpAsync(request);
        //        return Ok(result);
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return StatusCode(429, new { message = ex.Message });
        //    }
        //}

        //[HttpPost("verify-otp")]
        //public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        //{
        //    try
        //    {
        //        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        //        var result = await _authService.VerifyOtpAsync(request, ipAddress);
        //        return Ok(result);
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}


    }
}
