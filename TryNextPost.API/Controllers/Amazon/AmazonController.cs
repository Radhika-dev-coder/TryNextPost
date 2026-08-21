using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TryNextPost.Application.DTO.AmazonDto;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Infrastructure.Service;

namespace TryNextPost.API.Controllers.Amazon
{
    [Route("api/[controller]")]
    [ApiController]
    public class AmazonController : ControllerBase
    {
        private readonly IAmazonAuthService _amazonAuthService;
        private readonly IAmazonShippingService _amazonShippingService;

        public AmazonController( IAmazonAuthService amazonAuthService, IAmazonShippingService amazonShippingService)
        {
            _amazonAuthService = amazonAuthService;
            _amazonShippingService = amazonShippingService;
        }

        [HttpGet("token")]
        public async Task<IActionResult> GetToken(
        CancellationToken cancellationToken)
        {
            var token =
                await _amazonAuthService.GetAccessTokenAsync(
                    cancellationToken);

            return Ok(new
            {
                success = true,
                tokenLength = token.Length
            });
        }
        [HttpPost("rates")]
        public async Task<IActionResult> GetRates( [FromBody] AmazonGetRatesRequest request, CancellationToken cancellationToken)
        {
            var result = await _amazonShippingService.GetRatesAsync( request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("shipments")]
        public async Task<IActionResult> CreateShipment([FromBody] AmazonPurchaseShipmentRequest request, CancellationToken cancellationToken)
        {
            var result = await _amazonShippingService.PurchaseShipmentAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("shipments/book")]
        public async Task<IActionResult> BookShipment([FromBody] AmazonBookShipmentRequest request, CancellationToken cancellationToken)
        {
            var result = await _amazonShippingService.BookShipmentAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("shipments/{shipmentId}/label")]
        public async Task<IActionResult> GetLabel(string shipmentId, CancellationToken cancellationToken)
        {
            var result = await _amazonShippingService.GetLabelAsync(
                new AmazonGetLabelRequest { ShipmentId = shipmentId, LabelFormat = "PDF" },
                cancellationToken);
            return Ok(result);
        }

        [HttpGet("shipments/{shipmentId}/track")]
        public async Task<IActionResult> TrackShipment(string shipmentId, CancellationToken cancellationToken)
        {
            var result = await _amazonShippingService.TrackShipmentAsync(
                new AmazonTrackShipmentRequest { ShipmentId = shipmentId },
                cancellationToken);
            return Ok(result);
        }

        [HttpPost("shipments/{shipmentId}/cancel")]
        public async Task<IActionResult> CancelShipment(string shipmentId, [FromBody] string? reason, CancellationToken cancellationToken)
        {
            var result = await _amazonShippingService.CancelShipmentAsync(
                new AmazonCancelShipmentRequest { ShipmentId = shipmentId, Reason = reason },
                cancellationToken);
            return Ok(result);
        }
    }
}
