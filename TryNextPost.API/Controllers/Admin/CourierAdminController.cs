using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TryNextPost.Application.DTO.Admin;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.IServices.Interface.IAdmin;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.API.Controllers.Admin
{
    [Route("api/admin/couriers")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class CourierAdminController : ControllerBase
    {
        private readonly ICourierAdminService _courierAdminService;

        public CourierAdminController(ICourierAdminService courierAdminService)
        {
            _courierAdminService = courierAdminService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCouriers()
        {
            var result = await _courierAdminService.GetCouriersAsync();
            return Ok(new ApiResponse<List<CourierAdminDto>>
            {
                Success = true,
                Message = SystemMessage.CouriersFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpPut("{courierId:long}/cod-fee")]
        public async Task<IActionResult> UpdateCodFee(
            long courierId,
            [FromBody] UpdateCourierCodFeeRequest request)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            try
            {
                var result = await _courierAdminService.UpdateCodFeeAsync(courierId, request, adminId);
                return Ok(new ApiResponse<CourierAdminDto>
                {
                    Success = true,
                    Message = SystemMessage.CourierCodFeeUpdatedSuccess,
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
        }
    }
}