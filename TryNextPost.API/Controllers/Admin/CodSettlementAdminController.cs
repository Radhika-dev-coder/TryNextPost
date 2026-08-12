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
    [Route("api/admin/cod-settlement")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class CodSettlementAdminController : ControllerBase
    {
        private readonly ICodSettlementService _codSettlementService;

        public CodSettlementAdminController(ICodSettlementService codSettlementService)
        {
            _codSettlementService = codSettlementService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] CodSettlementAdminFilterRequest filter)
        {
            var result = await _codSettlementService.GetForAdminAsync(filter ?? new CodSettlementAdminFilterRequest());
            return Ok(new ApiResponse<CodSettlementAdminListResponse>
            {
                Success = true,
                Message = SystemMessage.CodRemittanceFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpPost("mark-settled")]
        public async Task<IActionResult> MarkSettled([FromBody] CodSettlementMarkSettledRequest request)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            try
            {
                var result = await _codSettlementService.MarkSettledAsync(adminId, request);
                return Ok(new ApiResponse<CodRemittanceListItemResponse>
                {
                    Success = true,
                    Message = SystemMessage.CodSettlementSettledSuccess,
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
