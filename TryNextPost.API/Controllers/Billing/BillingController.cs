using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TryNextPost.Application.DTO.Billing;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.IServices.Interface.IBilling;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.API.Controllers.Billing
{
    [Route("api/billing")]
    [ApiController]
    [Authorize(Roles = "Seller,SellerEmployee,SuperAdmin")]
    public class BillingController : ControllerBase
    {
        private readonly IBillingService _billingService;

        public BillingController(IBillingService billingService)
        {
            _billingService = billingService;
        }

        [HttpGet("shipping-charges")]
        public async Task<IActionResult> GetShippingCharges([FromQuery] ShipmentChargesFilterRequest filter)
        {
            var userId = RequireUserId();
            if (userId == null)
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _billingService.GetShipmentChargesAsync(userId, filter);
            return Ok(new ApiResponse<ShipmentChargesListResponse>
            {
                Success = true,
                Message = SystemMessage.ShipmentChargesFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpGet("cod/summary")]
        public async Task<IActionResult> GetCodSummary()
        {
            var userId = RequireUserId();
            if (userId == null)
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _billingService.GetCodSummaryAsync(userId);
            return Ok(new ApiResponse<CodRemittanceSummaryResponse>
            {
                Success = true,
                Message = SystemMessage.CodRemittanceSummaryFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpGet("cod/remittances")]
        public async Task<IActionResult> GetCodRemittances([FromQuery] CodRemittanceFilterRequest filter)
        {
            var userId = RequireUserId();
            if (userId == null)
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _billingService.GetCodRemittancesAsync(userId, filter);
            return Ok(new ApiResponse<CodRemittanceListResponse>
            {
                Success = true,
                Message = SystemMessage.CodRemittanceFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpGet("bank-accounts")]
        public async Task<IActionResult> GetBankAccounts()
        {
            var userId = RequireUserId();
            if (userId == null)
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _billingService.GetBankAccountsAsync(userId);
            return Ok(new ApiResponse<List<SellerBankAccountResponse>>
            {
                Success = true,
                Message = SystemMessage.CodBankDetailsFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpPost("bank-accounts")]
        public async Task<IActionResult> CreateBankAccount([FromBody] SellerBankAccountRequest request)
        {
            var userId = RequireUserId();
            if (userId == null)
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _billingService.CreateBankAccountAsync(userId, request);
            return Ok(new ApiResponse<SellerBankAccountResponse>
            {
                Success = true,
                Message = SystemMessage.CodBankDetailsSavedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpPut("bank-accounts/{id:long}")]
        public async Task<IActionResult> UpdateBankAccount(long id, [FromBody] SellerBankAccountRequest request)
        {
            var userId = RequireUserId();
            if (userId == null)
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _billingService.UpdateBankAccountAsync(userId, id, request);
            return Ok(new ApiResponse<SellerBankAccountResponse>
            {
                Success = true,
                Message = SystemMessage.CodBankDetailsUpdatedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpDelete("bank-accounts/{id:long}")]
        public async Task<IActionResult> DeleteBankAccount(long id)
        {
            var userId = RequireUserId();
            if (userId == null)
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            await _billingService.DeleteBankAccountAsync(userId, id);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = SystemMessage.CodBankDetailsDeletedSuccess,
                Data = null,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpGet("invoices")]
        public async Task<IActionResult> GetInvoices([FromQuery] InvoiceFilterRequest filter)
        {
            var userId = RequireUserId();
            if (userId == null)
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _billingService.GetInvoicesAsync(userId, filter);
            return Ok(new ApiResponse<InvoiceListResponse>
            {
                Success = true,
                Message = SystemMessage.InvoicesFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpGet("invoices/{id:long}/download")]
        public async Task<IActionResult> DownloadInvoice(long id)
        {
            var userId = RequireUserId();
            if (userId == null)
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var (content, fileName) = await _billingService.DownloadInvoiceCsvAsync(userId, id);
            return File(content, "text/csv", fileName);
        }

        private string? RequireUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
