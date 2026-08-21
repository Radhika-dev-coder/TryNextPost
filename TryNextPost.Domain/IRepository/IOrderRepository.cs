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
        Task<int> GetOrdersCountAsync(long sellerId, OrderStatus? statusFilter, OrderCategoryEnum? orderCategory = null);
        Task<Dictionary<OrderStatus, int>> GetStatusCountsBySellerAsync(long sellerId, OrderCategoryEnum? orderCategory = null);
        Task<Order?> GetByOrderRefAsync(string orderRef);

        Task<List<Order>> GetOrdersFilteredAsync(long sellerId, OrderFilterCriteria filter, OrderStatus? statusFilter);
        Task<int> GetOrdersFilteredCountAsync(long sellerId, OrderFilterCriteria filter, OrderStatus? statusFilter);
        Task<Order?> GetForShipmentAsync(long orderId, CancellationToken cancellationToken = default);
        Task<Order?> GetOrderWithItemsAndShipmentAsync(long orderId, string userId);

    }
}
