using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.DTO.Pincode;
using TryNextPost.Application.IServices.Interface;


namespace TryNextPost.API.Controllers.Common
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        private readonly IPincodeService _pincodeService;

        public CommonController(IPincodeService pincodeService)
        {
            _pincodeService = pincodeService;
        }

        [HttpGet("pincode/{pincode}")]
        public async Task<IActionResult> GetAddress(string pincode)
        {
            var result = await _pincodeService.GetAddressFromPincode(pincode);

            return Ok(ApiResponse<PincodeResponseDto>.SuccessResponse(result));
        }

        [HttpPost("reverse")]
        public async Task<IActionResult> GetAddress([FromBody] LocationRequestDto request)
        {
            var result = await _pincodeService
                .GetAddressFromCoordinates(request);

            return Ok(ApiResponse<LocationResponseDto>.SuccessResponse(result));
        }
    }
}
