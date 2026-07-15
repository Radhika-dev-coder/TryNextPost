using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.IRepository
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order);
        Task<Order> GetByIdAsync(long orderId);
        Task<List<Order>> GetBySellerIdAsync(long sellerId);
        Task SaveChangesAsync();
        Task UpdateAsync(Order order);
        Task UpdateOrderItem(OrderItem orderitem);

        Task<List<Order>> GetOrdersPagedAsync(long sellerId, int page, int pageSize, OrderStatus? statusFilter);
        Task<int> GetOrdersCountAsync(long sellerId, OrderStatus? statusFilter);
    }
}
