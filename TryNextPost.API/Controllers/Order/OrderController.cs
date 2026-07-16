using EllipticCurve.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.DTO.Order;
using TryNextPost.Application.IServices.Interface.IOrder;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;

namespace TryNextPost.API.Controllers.Order
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Seller,SuperAdmin")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("create-forward")]
        public async Task<IActionResult> CreateForwardOrder([FromBody] CreateForwardOrderRequest request)
        {

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var orderId = await _orderService.CreateForwardOrderAsync(request, userId);

            return Ok(new ApiResponse<long>
            {
                Success = true,
                Message = SystemMessage.OrderCreatedSuccess,
                Data = orderId
            });

        }

        [HttpPost("create-reverse")]
        public async Task<IActionResult> CreateReverseOrder([FromBody] CreateForwardOrderRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var orderId = await _orderService.CreateReverseOrderAsync(request, userId);
            return Ok(new ApiResponse<long>
            {
                Success = true,
                Message = SystemMessage.OrderCreatedSuccess,
                Data = orderId
            });
        }

        [HttpPost("create-reverse-qc")]
        public async Task<IActionResult> CreateReverseQCOrder([FromBody] CreateForwardOrderRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = SystemMessage.InvalidToken });

            var orderId = await _orderService.CreateReverseQCOrderAsync(request, userId);
            return Ok(new ApiResponse<long>
            {
                Success = true,
                Message = SystemMessage.OrderCreatedSuccess,
                Data = orderId
            });
        }

        [HttpGet("generate-order-ref")]
        public IActionResult GenerateOrderRef()
        {
            var orderRef = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            return Ok(new { orderRef });
        }

        [HttpPut("update-order/{orderId}")]
        public async Task<IActionResult> UpdateForwardOrder(long orderId, [FromBody] UpdateForwardOrderRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(SystemMessage.Unauthorized);

                await _orderService.UpdateOrderAsync(orderId, request, userId);
                return Ok(new { message = "Order updated successfully" });
        }

        [HttpDelete("CancelOrder/{orderId}")]
        public async Task<IActionResult> CancelOrder(long orderId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(SystemMessage.Unauthorized);

                await _orderService.CancelOrderAsync(orderId, userId);
                return Ok(new { message = "Order cancelled successfully" });
        }

        [HttpGet("GetOrderById/{OrderId}")]
        public async Task<IActionResult> GetOrderById([FromRoute] long OrderId)
        {

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var order = await _orderService.GetOrderByIdAsync(OrderId, userId);
            return Ok(new ApiResponse<OrderDetailResponse>
            {
                Success = true,
                Message = "Order fetched successfully",
                Data = order
            });
        }

        [HttpGet("all-orders")]
        public async Task<IActionResult> GetAllOrders(
        [FromQuery] string? tab = "all",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await _orderService.GetAllOrdersAsync(userId, page, pageSize, tab);

            return Ok(new ApiResponse<OrderListResponse>
            {
                Success = true,
                Message = SystemMessage.OrderFetchedSuccess,
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }
    }
}
