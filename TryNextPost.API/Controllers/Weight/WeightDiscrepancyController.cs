using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.DTO.Weight;
using TryNextPost.Application.IServices.Interface.IWeight;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.API.Controllers.Weight
{
    [Route("api/weight/discrepancy")]
    [ApiController]
    [Authorize(Roles = "Seller,SellerEmployee,SuperAdmin")]
    public class WeightDiscrepancyController : ControllerBase
    {
        private readonly IWeightDiscrepancyService _service;

        public WeightDiscrepancyController(IWeightDiscrepancyService service)
        {
            _service = service;
        }

        /// <summary>
        /// List weight discrepancies with filters, status tabs, and tab counts.
        /// Sellers see own records; SuperAdmin sees all.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] WeightDiscrepancyFilterRequest filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _service.GetListAsync(userId, User.IsInRole("SuperAdmin"), filter);
            return Ok(new ApiResponse<WeightDiscrepancyListResponse>
            {
                Success = true,
                Message = SystemMessage.WeightDiscrepancyFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpPost("{id:long}/accept")]
        public async Task<IActionResult> Accept(long id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _service.AcceptAsync(userId, User.IsInRole("SuperAdmin"), id);
            return Ok(new ApiResponse<WeightDiscrepancyListItemResponse>
            {
                Success = true,
                Message = SystemMessage.WeightDiscrepancyAccepted,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpPost("{id:long}/dispute")]
        public async Task<IActionResult> Dispute(long id, [FromBody] WeightDiscrepancyDisputeRequest? request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _service.DisputeAsync(
                userId,
                User.IsInRole("SuperAdmin"),
                id,
                request ?? new WeightDiscrepancyDisputeRequest());

            return Ok(new ApiResponse<WeightDiscrepancyListItemResponse>
            {
                Success = true,
                Message = SystemMessage.WeightDiscrepancyDisputed,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        /// <summary>SuperAdmin closes an open dispute.</summary>
        [HttpPost("{id:long}/close")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CloseDispute(long id, [FromBody] WeightDiscrepancyDisputeRequest? request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _service.CloseDisputeAsync(userId, id, request?.Remarks);
            return Ok(new ApiResponse<WeightDiscrepancyListItemResponse>
            {
                Success = true,
                Message = SystemMessage.WeightDiscrepancyClosed,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] WeightDiscrepancyFilterRequest filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var (content, fileName) = await _service.ExportCsvAsync(userId, User.IsInRole("SuperAdmin"), filter);
            return File(content, "text/csv", fileName);
        }
    }
}
