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
    [Route("api/admin/tds")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class TdsAdminController : ControllerBase
    {
        private readonly ITdsCertificateService _tdsCertificateService;

        public TdsAdminController(ITdsCertificateService tdsCertificateService)
        {
            _tdsCertificateService = tdsCertificateService;
        }

        [HttpGet("sellers")]
        public async Task<IActionResult> GetSellers()
        {
            var result = await _tdsCertificateService.GetSellerLookupAsync();
            return Ok(new ApiResponse<List<TdsSellerLookupDto>>
            {
                Success = true,
                Message = SystemMessage.TdsSellersFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCertificates([FromQuery] TdsCertificateFilterRequest filter)
        {
            var result = await _tdsCertificateService.GetForAdminAsync(filter ?? new TdsCertificateFilterRequest());
            return Ok(new ApiResponse<TdsCertificateListResponse>
            {
                Success = true,
                Message = SystemMessage.TdsCertificatesFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] TdsCertificateUploadRequest request)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            if (request.File == null || request.File.Length == 0)
                return BadRequest(new { message = SystemMessage.TdsCertificateFileRequired });

            try
            {
                var result = await _tdsCertificateService.UploadForAdminAsync(
                    adminId,
                    request.SellerId,
                    request.FinancialYear,
                    request.Quarter,
                    request.CertificateNumber,
                    request.Amount,
                    request.DeductorName,
                    request.DeductorTan,
                    request.Remarks,
                    request.File);

                return Ok(new ApiResponse<TdsCertificateListItemResponse>
                {
                    Success = true,
                    Message = SystemMessage.TdsCertificateUploadSuccess,
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

        [HttpGet("{id:long}/download")]
        public async Task<IActionResult> Download(long id)
        {
            var (content, fileName, contentType) = await _tdsCertificateService.DownloadForAdminAsync(id);
            return File(content, contentType, fileName);
        }

        [HttpPost("{id:long}/revoke")]
        public async Task<IActionResult> Revoke(long id)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            try
            {
                await _tdsCertificateService.RevokeForAdminAsync(adminId, id);
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = SystemMessage.TdsCertificateRevokedSuccess,
                    Data = null,
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
