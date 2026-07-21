using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.DTO.Employee;
using TryNextPost.Application.IServices.Interface.IEmployee;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.API.Controllers.Employee
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Seller,SuperAdmin")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet("permissions")]
        public async Task<IActionResult> GetAvailablePermissions()
        {
            var result = await _employeeService.GetAvailablePermissionsAsync();
            return Ok(new ApiResponse<List<string>>
            {
                Success = true,
                Message = SystemMessage.DataFound,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _employeeService.ListAsync(userId);
            return Ok(new ApiResponse<List<EmployeeResponse>>
            {
                Success = true,
                Message = SystemMessage.DataFound,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _employeeService.CreateAsync(userId, request);
            return Ok(new ApiResponse<EmployeeResponse>
            {
                Success = true,
                Message = "Employee created successfully.",
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpPut("{employeeId:long}")]
        public async Task<IActionResult> Update(long employeeId, [FromBody] UpdateEmployeeRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _employeeService.UpdateAsync(userId, employeeId, request);
            return Ok(new ApiResponse<EmployeeResponse>
            {
                Success = true,
                Message = "Employee updated successfully.",
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }
    }
}
