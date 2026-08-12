using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TryNextPost.Application.DTO;
using TryNextPost.Application.DTO.SellerKYC;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.IServices.Interface.SellerKYC;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
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
        private readonly ISurepassService _surepassServices;
        public SellerController(ISellerKycServices sellerKycervices, UserManager<ApplicationUser> userManager, ISurepassService surepassServices)
        {
            _sellerKycServices = sellerKycervices;
            _userManager = userManager;
            _surepassServices = surepassServices;
        }

        [HttpGet("my-orders")]
        public string GetMyOrders()
        {
            return "This is Seller Dashboard";
        }

        [HttpPost("Send-Aadhar-Otp")]
        public async Task<IActionResult> SendAadharOtp([FromBody] SendAadhaarOtpRequestDto dto)
        {

            var response = new BaseResponse<object>();
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    response.StatusCode = (int)ApiStatusCode.Unauthorized; ;
                    response.Success = false;
                    response.Data = null;
                    response.Message = SystemMessage.Unauthorized;
                    return BadRequest(response);

                }
                var res = await _sellerKycServices.SendOtpAadharKyc(dto, userId);
                return StatusCode((int)res.StatusCode, res);
            }
            catch (Exception ex)
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

            var response = new BaseResponse<object>();
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    response.StatusCode = (int)ApiStatusCode.Unauthorized; ;
                    response.Success = false;
                    response.Data = null;
                    response.Message = SystemMessage.Unauthorized;
                    return BadRequest(response);

                }
                var res = await _sellerKycServices.AddSellerKycAsync(dto, userId);
                return StatusCode((int)res.StatusCode, res);
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)ApiStatusCode.BadRequest;
                response.Success = false;
                response.Data = null;
                response.Message = ex.Message;
                return BadRequest(response);
            }

        }

        [HttpPost("Pan-KYC")]
        public async Task<IActionResult> PanKYC([FromBody] PanComprehensiveRequest request, CancellationToken cancellationToken)
        {
            var response = new BaseResponse<object>();
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    response.StatusCode = (int)ApiStatusCode.Unauthorized; ;
                    response.Success = false;
                    response.Data = null;
                    response.Message = SystemMessage.Unauthorized;
                    return BadRequest(response);

                }
                var res = await _surepassServices.VerifyPanAsync(request.id_number, userId, cancellationToken);
                return StatusCode((int)res.StatusCode, res);
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)ApiStatusCode.BadRequest;
                response.Success = false;
                response.Data = null;
                response.Message = ex.Message;
                return BadRequest(response);
            }

        }
    
        [HttpPost("Bank-KYC")]
        public async Task<IActionResult> BankKYC([FromBody] BankAccountVerificationRequest request)
        {
            var response = new BaseResponse<object>();
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    response.StatusCode = (int)ApiStatusCode.Unauthorized; ;
                    response.Success = false;
                    response.Data = null;
                    response.Message = SystemMessage.Unauthorized;
                    return BadRequest(response);

                }
                var res = await _surepassServices.VerifyBankAccountAsync(request, userId);
                return StatusCode((int)res.StatusCode, res);
            }
            catch (Exception ex)
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
