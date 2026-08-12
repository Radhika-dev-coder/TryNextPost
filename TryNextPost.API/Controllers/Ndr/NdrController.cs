using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.DTO.Ndr;
using TryNextPost.Application.IServices.Interface.INdr;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.API.Controllers.Ndr
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Seller,SellerEmployee,SuperAdmin")]
    public class NdrController : ControllerBase
    {
        private readonly INdrService _ndrService;

        public NdrController(INdrService ndrService)
        {
            _ndrService = ndrService;
        }

        /// <summary>
        /// List seller NDRs with StatusTab, SearchQuery, FromDate/ToDate filters and tab counts.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] NdrFilterRequest filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _ndrService.GetListAsync(userId, filter);
            return Ok(new ApiResponse<NdrListResponse>
            {
                Success = true,
                Message = SystemMessage.NdrFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        /// <summary>
        /// Take action on an NDR (Phase-1.5: Reattempt → ActionRequested, or Rto/MarkRto → Rto + RTOS row).
        /// Local-only — does not call courier APIs.
        /// </summary>
        [HttpPost("{ndrId:long}/action")]
        public async Task<IActionResult> TakeAction(long ndrId, [FromBody] NdrActionRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            if (request == null)
                return BadRequest(new { message = SystemMessage.RequestBodyNull });

            var result = await _ndrService.TakeActionAsync(userId, ndrId, request);
            return Ok(new ApiResponse<NdrListItemResponse>
            {
                Success = true,
                Message = SystemMessage.NdrActionSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }
    }
}
