using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.DTO.Settlement;
using TryNextPost.Application.IServices.Interface.ISettlement;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.API.Controllers.Admin
{
    [Route("api/admin/courier-settlement")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class CourierSettlementAdminController : ControllerBase
    {
        private readonly ICourierSettlementService _settlementService;

        public CourierSettlementAdminController(ICourierSettlementService settlementService)
        {
            _settlementService = settlementService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] long courierId,
            [FromQuery] DateTime periodFrom,
            [FromQuery] DateTime periodTo)
        {
            var result = await _settlementService.GetSummaryAsync(courierId, periodFrom, periodTo);
            return Ok(new ApiResponse<CourierSettlementSummaryResponse>
            {
                Success = true,
                Message = SystemMessage.CourierSettlementFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }
      
        [HttpGet]
        public async Task<IActionResult> GetSettlements(
            [FromQuery] long? courierId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var result = await _settlementService.GetSettlementsAsync(courierId, from, to);
            return Ok(new ApiResponse<List<CourierSettlementResponse>>
            {
                Success = true,
                Message = SystemMessage.CourierSettlementFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSettlement([FromBody] CreateCourierSettlementRequest request)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _settlementService.CreateSettlementBatchAsync(request, adminId);
            return Ok(new ApiResponse<CourierSettlementResponse>
            {
                Success = true,
                Message = SystemMessage.CourierSettlementCreatedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }
        [HttpPost("mark-paid")]
        public async Task<IActionResult> MarkPaid([FromBody] MarkCourierSettlementPaidRequest request)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _settlementService.MarkAsPaidAsync(request, adminId);
            return Ok(new ApiResponse<CourierSettlementResponse>
            {
                Success = true,
                Message = SystemMessage.CourierSettlementPaidSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }
    }
}


