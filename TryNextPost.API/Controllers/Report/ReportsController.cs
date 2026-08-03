using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.DTO.Report;
using TryNextPost.Application.IServices.Interface.IReport;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.API.Controllers.Report
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Seller,SellerEmployee,SuperAdmin")]
    public class ReportsController : ControllerBase
    {
        private readonly ICustomReportService _customReportService;
        public ReportsController(ICustomReportService customReportService)
        {
            _customReportService = customReportService;
        }

        [HttpPost("custom")]
        public async Task<IActionResult> GenerateCustomReport([FromBody] CustomReportRequest request)
        {
            var userId = RequireUserId();
            if (userId == null)
                return Unauthorized(new { message = SystemMessage.InvalidToken });
            var (content, fileName, contentType, _) =
                await _customReportService.GenerateCustomReportAsync(userId, request);
            return File(content, contentType, fileName);
        }


        // for all tab
        [HttpPost("export")]
        public async Task<IActionResult> ExportReport([FromBody] ReportRequest request)
        {
            var userId = RequireUserId();
            if (userId == null)
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _customReportService.ExportReportAsync(userId, request);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Export started successfully",
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpGet("export-history")]
        public async Task<IActionResult> GetExportHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        {
            var userId = RequireUserId();
            if (userId == null)
                return Unauthorized(new { message = SystemMessage.InvalidToken });
            var result = await _customReportService.GetExportHistoryAsync(userId, page, pageSize);
            return Ok(new ApiResponse<ExportHistoryListResponse>
            {
                Success = true,
                Message = SystemMessage.ExportHistoryFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpGet("export-history/{id:long}/download")]
        public async Task<IActionResult> DownloadExport(long id)
        {
            var userId = RequireUserId();
            if (userId == null)
                return Unauthorized(new { message = SystemMessage.InvalidToken });
            var (content, fileName) = await _customReportService.DownloadExportAsync(userId, id);
            return File(content, "text/csv", fileName);
        }
        private string? RequireUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value;



        [HttpGet("shipment-summary")]
        public async Task<IActionResult> GetShipmentSummary([FromQuery] ShipmentSummaryRequest request)
        {
            var userId = RequireUserId();
            if (userId == null)
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _customReportService.GetShipmentSummaryAsync(userId, request);

            return Ok(new ApiResponse<ShipmentSummaryResponse>
            {
                Success = true,
                Message = SystemMessage.ShipmentSummaryFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpGet("daily-summary")]
        public async Task<IActionResult> GetDailySummary([FromQuery] DailySummaryRequest request)
        {
            var userId = RequireUserId();
            if (userId == null)
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _customReportService.GetDailySummaryDataAsync(userId, request);

            return Ok(new ApiResponse<List<DailySummaryResponse>>
            {
                Success = true,
                Message = SystemMessage.DailySummaryFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

    }
}
