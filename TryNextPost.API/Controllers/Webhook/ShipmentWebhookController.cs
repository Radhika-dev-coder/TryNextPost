using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.DTO.Shipment;
using TryNextPost.Application.IServices.Interface.IShipment;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.API.Controllers.Webhook
{
    /// <summary>
    /// Courier tracking webhook skeleton. Secure with IP allowlist / shared secret later.
    /// </summary>
    [Route("api/webhook/shipment")]
    [ApiController]
    [AllowAnonymous]
    public class ShipmentWebhookController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;

        public ShipmentWebhookController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        /// <summary>
        /// Ingest a tracking status update (updates Shipment.Status + ShipmentTracking row).
        /// </summary>
        [HttpPost("tracking")]
        public async Task<IActionResult> Tracking(
            [FromBody] ShipmentTrackingWebhookRequest request,
            CancellationToken cancellationToken)
        {
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
