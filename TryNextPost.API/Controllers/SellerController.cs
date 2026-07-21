using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TryNextPost.Application.DTO;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;
using TryNextPost.Infrastructure.Identity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TryNextPost.API.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Authorize]
    public class SellerController : ControllerBase
    {
        private readonly ISellerKycServices _sellerKycServices;
        private readonly UserManager<ApplicationUser> _userManager;
        public SellerController(ISellerKycServices sellerKycervices, UserManager<ApplicationUser> userManager)
        {
            _sellerKycServices = sellerKycervices;
            _userManager = userManager;
        }

        [HttpGet("my-orders")]
        public string GetMyOrders()
        {
            return "This is Seller Dashboard";
        }

        [HttpPost("Send-Aadhar-Otp")]
        public async Task<IActionResult> SendAadharOtp([FromBody] SendAadhaarOtpRequestDto dto)
        {
            
            var response = new  BaseResponse<object>();
            try
            {
                var userId = _userManager.GetUserId(User);
                if(userId == null)
                {
                    response.StatusCode = (int)ApiStatusCode.Unauthorized; ;
                    response.Success = false;
                    response.Data = null;
                    response.Message = SystemMessage.Unauthorized;
                    return BadRequest(response);

                }
                var res = await _sellerKycServices.SendOtpAadharKyc(dto,userId);
                return StatusCode((int)res.StatusCode, res);
            }
            catch(Exception ex)
            {
                response.StatusCode = (int)ApiStatusCode.BadRequest;
                response.Success = false;
                response.Data = null;
                response.Message = ex.Message;
                return BadRequest(response);
            }

        }

        [HttpPost("Verification-Aadhar-Otp")]
        public async Task<IActionResult> VerificationAadharOtp([FromBody] VerifyAadhaarOtpRequestDto dto)
        {
            
            var response = new  BaseResponse<object>();
            try
            {
                var userId = _userManager.GetUserId(User);
                if(userId == null)
                {
                    response.StatusCode = (int)ApiStatusCode.Unauthorized; ;
                    response.Success = false;
                    response.Data = null;
                    response.Message = SystemMessage.Unauthorized;
                    return BadRequest(response);

                }
                var res = await _sellerKycServices.AddSellerKycAsync(dto,userId);
                return StatusCode((int)res.StatusCode, res);
            }
            catch(Exception ex)
            {
                response.StatusCode = (int)ApiStatusCode.BadRequest;
                response.Success = false;
                response.Data = null;
                response.Message = ex.Message;
                return BadRequest(response);
            }

        }
    }
}
