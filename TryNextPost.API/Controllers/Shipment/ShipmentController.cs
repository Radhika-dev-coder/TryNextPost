using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.DTO.Shipment;
using TryNextPost.Application.IServices.Interface.IShipment;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.API.Controllers.Shipment
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Seller,SuperAdmin")]
    public class ShipmentController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;

        public ShipmentController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        /// <summary>
        /// List seller shipments with optional NimbusPost-style StatusTab filter.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetShipments([FromQuery] ShipmentFilterRequest filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _shipmentService.GetShipmentsAsync(userId, filter);
            return Ok(new ApiResponse<ShipmentListResponse>
            {
                Success = true,
                Message = SystemMessage.ShipmentsFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        /// <summary>
        /// Get courier rate options for an order (stub adapters OK when credentials missing).
        /// </summary>
        [HttpGet("rates/{orderId:long}")]
        public async Task<IActionResult> GetRates(long orderId, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var rates = await _shipmentService.GetRatesAsync(orderId, userId, cancellationToken);
            return Ok(new ApiResponse<GetShipmentRatesResponse>
            {
                Success = true,
                Message = SystemMessage.ShipmentRatesFetchedSuccess,
                Data = rates,
                StatusCode = ApiStatusCode.Success
            });
        }

        /// <summary>
        /// Confirm / book shipment with selected courier + rate charge (wallet debit).
        /// </summary>
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmShipment(
            [FromBody] ConfirmShipmentRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _shipmentService.ConfirmShipmentAsync(request, userId, cancellationToken);
            return Ok(new ApiResponse<ConfirmShipmentResponse>
            {
                Success = true,
                Message = SystemMessage.ShipmentBookedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        /// <summary>
        /// Get packing label for a shipment (stub label when courier credentials empty).
        /// </summary>
        [HttpGet("{shipmentId:long}/label")]
        public async Task<IActionResult> GetLabel(long shipmentId, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _shipmentService.GetLabelAsync(shipmentId, userId, cancellationToken);
            return Ok(new ApiResponse<ShipmentLabelResponse>
            {
                Success = true,
                Message = SystemMessage.ShipmentLabelFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        /// <summary>
        /// Cancel a booked / pending-pickup shipment.
        /// </summary>
        [HttpPost("{shipmentId:long}/cancel")]
        public async Task<IActionResult> Cancel(
            long shipmentId,
            [FromBody] CancelShipmentRequest? request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _shipmentService.CancelShipmentAsync(
                shipmentId,
                request ?? new CancelShipmentRequest(),
                userId,
                cancellationToken);

            return Ok(new ApiResponse<CancelShipmentResponse>
            {
                Success = true,
                Message = SystemMessage.ShipmentCancelledSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        /// <summary>
        /// Track a shipment via courier adapter + local tracking history.
        /// </summary>
        [HttpGet("{shipmentId:long}/track")]
        public async Task<IActionResult> Track(long shipmentId, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var result = await _shipmentService.TrackShipmentAsync(shipmentId, userId, cancellationToken);
            return Ok(new ApiResponse<ShipmentTrackResponse>
            {
                Success = true,
                Message = SystemMessage.ShipmentTrackedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }

        /// <summary>
        /// Get the latest shipment for an order (seller-owned).
        /// </summary>
        [HttpGet("by-order/{orderId:long}")]
        public async Task<IActionResult> GetByOrder(long orderId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var shipment = await _shipmentService.GetShipmentByOrderIdAsync(orderId, userId);
            return Ok(new ApiResponse<ShipmentDetailResponse>
            {
                Success = true,
                Message = SystemMessage.DataFound,
                Data = shipment,
                StatusCode = ApiStatusCode.Success
            });
        }
    }
}
