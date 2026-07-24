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
    [Route("api/weight/freeze")]
    [ApiController]
    [Authorize(Roles = "Seller,SellerEmployee,SuperAdmin")]
    public class WeightFreezeController : ControllerBase
    {
        private readonly IWeightFreezeService _service;

        public WeightFreezeController(IWeightFreezeService service)
        {
            _service = service;
        }

        /// <summary>
        /// List product weight freeze requests. Sellers see own; SuperAdmin sees all.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] WeightFreezeFilterRequest filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _service.GetListAsync(userId, User.IsInRole("SuperAdmin"), filter);
            return Ok(new ApiResponse<WeightFreezeListResponse>
            {
                Success = true,
                Message = SystemMessage.WeightFreezeFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        /// <summary>Seller creates a Requested weight freeze.</summary>
        [HttpPost]
        [Authorize(Roles = "Seller,SellerEmployee")]
        public async Task<IActionResult> Create([FromBody] CreateWeightFreezeRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            if (request == null)
                return BadRequest(new { message = SystemMessage.RequestBodyNull });

            var result = await _service.CreateAsync(userId, request);
            return Ok(new ApiResponse<WeightFreezeListItemResponse>
            {
                Success = true,
                Message = SystemMessage.WeightFreezeCreatedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        /// <summary>SuperAdmin accept/reject a freeze request.</summary>
        [HttpPost("{id:long}/action")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> TakeAction(long id, [FromBody] WeightFreezeActionRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            if (request == null)
                return BadRequest(new { message = SystemMessage.RequestBodyNull });

            var result = await _service.TakeActionAsync(userId, id, request);
            return Ok(new ApiResponse<WeightFreezeListItemResponse>
            {
                Success = true,
                Message = SystemMessage.WeightFreezeActionSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpPost("{id:long}/unfreeze")]
        public async Task<IActionResult> Unfreeze(long id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _service.UnfreezeAsync(userId, User.IsInRole("SuperAdmin"), id);
            return Ok(new ApiResponse<WeightFreezeListItemResponse>
            {
                Success = true,
                Message = SystemMessage.WeightFreezeUnfrozenSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpPost("import")]
        [Authorize(Roles = "Seller,SellerEmployee")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            if (file == null || file.Length == 0)
                return BadRequest(new { message = SystemMessage.WeightFreezeImportFileRequired });

            var result = await _service.ImportCsvAsync(userId, file);
            return Ok(new ApiResponse<WeightFreezeImportResult>
            {
                Success = true,
                Message = SystemMessage.WeightFreezeImportSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        [HttpGet("import/sample")]
        [Authorize(Roles = "Seller,SellerEmployee")]
        public IActionResult DownloadImportSample()
        {
            var (content, fileName) = _service.GetImportSampleCsv();
            return File(content, "text/csv", fileName);
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] WeightFreezeFilterRequest filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var (content, fileName) = await _service.ExportCsvAsync(userId, User.IsInRole("SuperAdmin"), filter);
            return File(content, "text/csv", fileName);
        }
    }
}
