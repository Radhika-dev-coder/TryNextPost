using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.DTO.Wallet;
using TryNextPost.Application.IServices.Interface.IWallet;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.API.Controllers.Wallet
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Seller,SuperAdmin")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _walletService.GetOrCreateBalanceAsync(userId);
            return Ok(new ApiResponse<WalletBalanceResponse>
            {
                Success = true,
                Message = SystemMessage.WalletFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        /// <summary>
        /// Creates a Razorpay order for wallet recharge (Seller). Returns keyId + gatewayOrderId for Checkout.
        /// </summary>
        [HttpPost("recharge")]
        public async Task<IActionResult> Recharge([FromBody] WalletRechargeRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _walletService.CreateRechargeAsync(userId, request);
            return Ok(new ApiResponse<WalletRechargeResponse>
            {
                Success = true,
                Message = SystemMessage.WalletRechargeCreated,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        /// <summary>
        /// Optional frontend return path after Razorpay Checkout success (verify signature + credit).
        /// Prefer webhook for production; this covers immediate UX after payment.
        /// </summary>
        [HttpPost("verify-payment")]
        public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _walletService.VerifyPaymentAsync(userId, request);
            return Ok(new ApiResponse<VerifyPaymentResponse>
            {
                Success = true,
                Message = result.AlreadyProcessed
                    ? SystemMessage.WalletPaymentAlreadyProcessed
                    : SystemMessage.WalletPaymentVerified,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        /// <summary>
        /// Razorpay payment webhook. Configure URL in Razorpay Dashboard → Webhooks.
        /// Local tip: expose with ngrok and set endpoint to https://&lt;ngrok&gt;/api/Wallet/payment-webhook
        /// Header: X-Razorpay-Signature
        /// </summary>
        [AllowAnonymous]
        [HttpPost("payment-webhook")]
        public async Task<IActionResult> PaymentWebhook()
        {
            using var reader = new StreamReader(Request.Body);
            var rawBody = await reader.ReadToEndAsync();
            var signature = Request.Headers["X-Razorpay-Signature"].FirstOrDefault();

            var result = await _walletService.HandleWebhookAsync(rawBody, signature);
            return Ok(new ApiResponse<PaymentWebhookResponse>
            {
                Success = true,
                Message = result.Message,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        /// <summary>
        /// Manual credit for admin/test only. Prefer POST /api/Wallet/recharge + Razorpay in production.
        /// Restricted to SuperAdmin. Body.userId = seller/owner whose wallet to credit (e.g. TNP-000002).
        /// </summary>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("credit")]
        public async Task<IActionResult> Credit([FromBody] WalletCreditRequest request)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _walletService.CreditAsync(request.UserId.Trim(), request, performedBy: adminId);
            return Ok(new ApiResponse<WalletBalanceResponse>
            {
                Success = true,
                Message = SystemMessage.WalletCreditSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }
    }
}
