using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using TryNextPost.Application.DTO;
using TryNextPost.Application.DTO.Auth;
using TryNextPost.Application.IServices;
using TryNextPost.Application.Services.Interface;
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
        [HttpPost("Register")]
        public async Task<IActionResult> Register(SellerDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    message = "Invalid Request Data",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }
            var result = await _authService.RegisterAsync(dto);
            if (!result.Success)
            {
                return Conflict(new
                {
                    Success = false,
                    messag = result.Message,
                    errors = result.Errors
                });
            }

            return Ok(new
            {
                Success = true,
                message = "User Registered Successfully",
                data = result.Data
            });

        }

        [HttpPost("check-email")]
        public async Task<IActionResult> CheckEmail(string email)
        {
            var exists = await _authService.CheckEmailAsync(email);
            return Ok(new { exists });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("verify-login-otp")]
        public async Task<IActionResult> VerifyLoginOtp(VerifyOtpRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var result = await _authService.VerifyOtpAsync(request, ipAddress);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest request)
        {
            try
            {
                var result = await _authService.ResendOtpAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(429, new { message = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] TryNextPost.Application.DTO.Auth.ForgotPasswordRequest request)
        {
            try
            {
                var result = await _authService.ForgotPasswordAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(429, new { message = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] TryNextPost.Application.DTO.Auth.ResetPasswordRequest request)
        {
            try
            {
                var message = await _authService.ResetPasswordAsync(request);
                return Ok(new { message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
