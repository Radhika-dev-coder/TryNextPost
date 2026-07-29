using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.DTO.Dashboard;
using TryNextPost.Application.IServices.Interface.IDashboard;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.API.Controllers.Admin
{
    [Route("api/admin/dashboard")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class DashboardAdminController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardAdminController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSuperAdminDashboard()
        {
            var result = await _dashboardService.GetSuperAdminDashboardAsync();
            return Ok(new ApiResponse<SuperAdminDashboardResponse>
            {
                Success = true,
                Message = SystemMessage.DashboardFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }
    }
}
