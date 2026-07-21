using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.DTO.Order;
using TryNextPost.Application.IServices.Interface.IOrder;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.API.Controllers.Order
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Seller,SellerEmployee,SuperAdmin")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private string? GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpPost("create-forward")]
        public async Task<IActionResult> CreateForwardOrder([FromBody] CreateForwardOrderRequest request)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(SystemMessage.InvalidToken);

            var orderId = await _orderService.CreateForwardOrderAsync(request, userId);

            return Ok(new ApiResponse<long>
            {
                Success = true,
                Message = SystemMessage.OrderCreatedSuccess,
                Data = orderId
            });
        }

        [HttpPost("create-reverse")]
        public async Task<IActionResult> CreateReverseOrder([FromBody] CreateReverseOrderRequest request)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(SystemMessage.InvalidToken);

            var orderId = await _orderService.CreateReverseOrderAsync(request, userId);

            return Ok(new ApiResponse<long>
            {
                Success = true,
                Message = SystemMessage.OrderCreatedSuccess,
                Data = orderId
            });
        }

        [HttpPost("create-reverse-qc")]
        public async Task<IActionResult> CreateReverseQCOrder([FromBody] CreateReverseQcOrderRequest request)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(SystemMessage.InvalidToken);

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
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(SystemMessage.Unauthorized);

            await _orderService.UpdateOrderAsync(orderId, request, userId);

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = SystemMessage.OrderUpdatedSuccess,
                Data = null
            });
        }

        [HttpDelete("cancel-order/{orderId}")]
        public async Task<IActionResult> CancelOrder(long orderId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(SystemMessage.Unauthorized);

            await _orderService.CancelOrderAsync(orderId, userId);

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = SystemMessage.OrderCancelledSuccess,
                Data = null
            });
        }

        [HttpGet("get-order-by-id/{orderId}")]
        public async Task<IActionResult> GetOrderById(long orderId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(SystemMessage.Unauthorized);

            var order = await _orderService.GetOrderByIdAsync(orderId, userId);

            return Ok(new ApiResponse<OrderDetailResponse>
            {
                Success = true,
                Message = "Order fetched successfully",
                Data = order
            });
        }

        [HttpGet("all-orders")]
        public async Task<IActionResult> GetAllOrders([FromQuery] OrderFilterRequest filter)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(SystemMessage.Unauthorized);

            var result = await _orderService.GetAllOrdersAsync(userId, filter);

            return Ok(new ApiResponse<OrderListResponse>
            {
                Success = true,
                Message = "Orders fetched successfully",
                Data = result,
                StatusCode = ApiStatusCode.Success
            });
        }
    }
}