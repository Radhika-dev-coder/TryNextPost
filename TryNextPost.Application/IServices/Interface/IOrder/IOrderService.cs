using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Order;

namespace TryNextPost.Application.IServices.Interface.IOrder
{
    public interface IOrderService
    {
        Task<long> CreateForwardOrderAsync(CreateForwardOrderRequest request, string userId);
        Task UpdateOrderAsync(long orderId, UpdateForwardOrderRequest request, string userId);

        Task CancelOrderAsync(long orderId, string userId);

        Task<OrderDetailResponse> GetOrderByIdAsync(long OrderId,string userId);
        Task<List<OrderDetailResponse>> GetMyOrdersAsync(string userId);

        Task<long> CreateReverseOrderAsync(CreateForwardOrderRequest request, string userId);
        Task<long> CreateReverseQCOrderAsync(CreateForwardOrderRequest request, string userId);
    }
}
