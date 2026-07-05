using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TryNextPost.Application.DTO;
using TryNextPost.Application.DTO.Auth;
using TryNextPost.Application.IServices;

namespace TryNextPost.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly  IAuthService _authService;
        private readonly IIdentityService _identityService;

        public AuthController(IAuthService authService,IIdentityService identityService)
        {
            _authService = authService;
            _identityService = identityService;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(SellerDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if(!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("GetSeller/{UserId}")]
        public async Task<IActionResult> GetSellerById(string UserId)
        {
            var seller = await _identityService.GetSellerById(UserId);

            if(seller == null)
            {
                return NotFound(new 
                { 
                  Success = false,
                  Message = "Seller Not Found"
                });

            }
            return Ok(new
            {
                Success = true,
                Data = seller
            });
        }

        [HttpPost("GetSellerList")]
        public async Task<IActionResult> GetSellerList()
        {
            var sellers = await _identityService.GetSellerList();
            if(sellers == null || !sellers.Any())
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "No Sellers Found"
                });
            }
            return Ok(new
            {
                Success=true,
                Data = sellers
            });
        }
    }
}
