using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TryNextPost.Application.DTO.Default;
using TryNextPost.Application.IServices.Interface.Default;

namespace TryNextPost.API.Controllers.Default
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;
        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpPost("pickup")]
        public async Task<IActionResult> AddPickupAddress([FromBody] AddPickupAddressRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid authentication token" });

                var addressId = await _addressService.AddPickupAddressAsync(request, userId);
                return Ok(new { message = "Pickup address added successfully", addressId });
             
        }

        [HttpGet("pickup")]
        public async Task<IActionResult> GetPickupAddresses()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid authentication token" });

            var addresses = await _addressService.GetPickupAddressesAsync(userId);
            return Ok(addresses);
        }

        [HttpPut("pickup/{addressId}")]
        public async Task<IActionResult> UpdatePickupAddress(long addressId, [FromBody] UpdatePickupAddressRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid authentication token" });

                await _addressService.UpdatePickupAddressAsync(addressId, request, userId);
                return Ok(new { message = "Address updated successfully" });

        }

        [HttpDelete("pickup/{addressId}")]
        public async Task<IActionResult> DeletePickupAddress(long addressId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid authentication token" });

                await _addressService.DeletePickupAddressAsync(addressId, userId);
                return Ok(new { message = "Address deleted successfully" });
        }

        [HttpGet("pickup/{addressId}")]
        public async Task<IActionResult> GetPickupAddressById(long addressId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid authentication token" });

                var address = await _addressService.GetPickupAddressByIdAsync(addressId, userId);
                return Ok(address);
        }
    }
}
