using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.DTO.Dashboard;
using TryNextPost.Application.IServices.Interface.IDashboard;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Seller,SellerEmployee")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("seller")]
        public async Task<IActionResult> GetSellerDashboard()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = SystemMessage.InvalidToken,
                    StatusCode = ApiStatusCode.Unauthorized
                });

            var result = await _dashboardService.GetSellerDashboardAsync(userId);
            return Ok(new ApiResponse<SellerDashboardResponse>
            {
                Success = true,
                Message = SystemMessage.DashboardFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }
    }
}
