using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.DTO.Shipment;
using TryNextPost.Application.IServices.Interface.IShipment;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.API.Controllers.Webhook
{
    [Route("api/webhook/shipment")]
    [ApiController]
    [AllowAnonymous]
    public class ShipmentWebhookController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;
        private readonly ILogger<ShipmentWebhookController> _logger;

        public ShipmentWebhookController(IShipmentService shipmentService, ILogger<ShipmentWebhookController> logger)
        {
            _shipmentService = shipmentService;
            _logger = logger;
        }

        [HttpPost("tracking")]
        public async Task<IActionResult> Tracking(
            [FromBody] ShipmentTrackingWebhookRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Tracking webhook received for AWB: {Awb}", request?.AwbNumber);

            var result = await _shipmentService.ProcessTrackingWebhookAsync(request, cancellationToken);
            return Ok(new ApiResponse<ShipmentTrackingWebhookResponse>
            {
                Success = true,
                Message = SystemMessage.TrackingWebhookAccepted,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }
    }
}
