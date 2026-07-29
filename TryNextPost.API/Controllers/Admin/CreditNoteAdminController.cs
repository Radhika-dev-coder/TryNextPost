using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TryNextPost.Application.DTO.Billing;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.IServices.Interface.IBilling;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.API.Controllers.Admin
{
    [Route("api/admin/credit-notes")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class CreditNoteAdminController : ControllerBase
    {
        private readonly ICreditNoteService _creditNoteService;

        public CreditNoteAdminController(ICreditNoteService creditNoteService)
        {
            _creditNoteService = creditNoteService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] CreditNoteFilterRequest filter)
        {
            var result = await _creditNoteService.GetForAdminAsync(
                filter ?? new CreditNoteFilterRequest());
            return Ok(new ApiResponse<CreditNoteListResponse>
            {
                Success = true,
                Message = SystemMessage.CreditNotesFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpGet("invoices")]
        public async Task<IActionResult> GetSellerInvoices(
            [FromQuery] long sellerId,
            [FromQuery] InvoiceFilterRequest filter)
        {
            if (sellerId <= 0)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = SystemMessage.TdsCertificateSellerRequired,
                    StatusCode = ApiStatusCode.BadRequest
                });

            var result = await _creditNoteService.GetInvoicesForAdminAsync(sellerId, filter ?? new InvoiceFilterRequest());
            return Ok(new ApiResponse<InvoiceListResponse>
            {
                Success = true,
                Message = SystemMessage.InvoicesFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreditNoteCreateRequest request)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            try
            {
                var result = await _creditNoteService.CreateForAdminAsync(adminId, request);
                return Ok(new ApiResponse<CreditNoteListItemResponse>
                {
                    Success = true,
                    Message = SystemMessage.CreditNoteCreatedSuccess,
                    Data = result,
                    StatusCode = ApiStatusCode.Success
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    StatusCode = ApiStatusCode.BadRequest
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    StatusCode = ApiStatusCode.NotFound
                });
            }
        }
    }
}
