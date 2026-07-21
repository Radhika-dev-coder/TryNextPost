using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;


namespace TryNextPost.Domain.IRepository
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order);
        Task<Order?> GetByIdAsync(long orderId);
        Task<List<Order>> GetBySellerIdAsync(long sellerId);
        Task SaveChangesAsync();
        Task UpdateAsync(Order order);
        Task UpdateOrderItem(OrderItem orderitem);
        Task<int> GetOrdersCountAsync(long sellerId, OrderStatus? statusFilter);
        Task<Order?> GetByOrderRefAsync(string orderRef);

        Task<List<Order>> GetOrdersFilteredAsync(long sellerId, OrderFilterCriteria filter, OrderStatus? statusFilter);
        Task<int> GetOrdersFilteredCountAsync(long sellerId, OrderFilterCriteria filter, OrderStatus? statusFilter);
    }
}
