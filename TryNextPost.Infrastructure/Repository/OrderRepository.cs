using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.IRepository;
using Microsoft.EntityFrameworkCore;
using TryNextPost.Infrastructure.AppDbContexts;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.Common;

namespace TryNextPost.Infrastructure.Repository
{

    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Order order)
        {
           await _context.Orders.AddAsync(order);
        }

        public async Task<Order?> GetByIdAsync(long orderId)
        {
            return await _context.Orders
           .Include(o => o.OrderItems)
           .Include(o => o.ReverseQcDetail)
            .ThenInclude(qc => qc.Images)
           .FirstOrDefaultAsync(o => o.OrderId == orderId && o.IsActive == true);
        }

        public async Task<Order?> GetByOrderRefAsync(string orderRef)
        {
           return await _context.Orders.FirstOrDefaultAsync(o => o.OrderRef == orderRef && o.IsActive== true);
        }

        public async Task<List<Order>> GetBySellerIdAsync(long sellerId)
        {
            return await _context.Orders.Include(o => o.OrderItems).
                Where(o => o.SellerId == sellerId && o.IsActive == true).ToListAsync();
        }

        public async Task<int> GetOrdersCountAsync(long sellerId, OrderStatus? statusFilter, OrderCategoryEnum? orderCategory = null)
        {
            var query = _context.Orders
           .Where(o => o.SellerId == sellerId && o.IsActive == true);

            if (statusFilter.HasValue)
                query = query.Where(o => o.Status == statusFilter.Value);

            if (orderCategory.HasValue)
                query = query.Where(o => o.OrderCategory == orderCategory.Value);

            return await query.CountAsync();
        }

        public async Task<Dictionary<OrderStatus, int>> GetStatusCountsBySellerAsync(long sellerId, OrderCategoryEnum? orderCategory = null)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Where(o => o.SellerId == sellerId && o.IsActive == true);

            if (orderCategory.HasValue)
                query = query.Where(o => o.OrderCategory == orderCategory.Value);

            return await query
                .GroupBy(o => o.Status)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);
        }

        public async Task<List<Order>> GetOrdersFilteredAsync(long sellerId, OrderFilterCriteria filter, OrderStatus? statusFilter)
        {
            var query = BuildFilterQuery(sellerId, filter, statusFilter, includeItems: true);

            return await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();
        }

        public async Task<int> GetOrdersFilteredCountAsync(long sellerId, OrderFilterCriteria filter, OrderStatus? statusFilter)
        {
            var query = BuildFilterQuery(sellerId, filter, statusFilter, includeItems: false);
            return await query.CountAsync();
        }

        private IQueryable<Order> BuildFilterQuery(long sellerId, OrderFilterCriteria filter, OrderStatus? statusFilter, bool includeItems)
        {
            IQueryable<Order> query = _context.Orders.AsNoTracking()
                .Where(o => o.SellerId == sellerId && o.IsActive == true);

            if (includeItems)
                query = query.Include(o => o.OrderItems);

            if (statusFilter.HasValue)
                query = query.Where(o => o.Status == statusFilter.Value);

            if (filter.OrderCategory.HasValue)
                query = query.Where(o => o.OrderCategory == filter.OrderCategory.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(o => o.OrderDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(o => o.OrderDate <= filter.ToDate.Value);

            if (!string.IsNullOrEmpty(filter.OrderIds))
            {
                var ids = filter.OrderIds.Split(',').Select(x => long.Parse(x.Trim())).ToList();
                query = query.Where(o => ids.Contains(o.OrderId));
            }

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                var search = filter.SearchQuery.Trim();
                query = query.Where(o => o.OrderRef.Contains(search)
                                       || o.CustomerName.Contains(search)
                                       || o.CustomerMobile.Contains(search));
            }

            if (!string.IsNullOrEmpty(filter.ProductName))
                query = query.Where(o => o.OrderItems.Any(oi => oi.ProductName.Contains(filter.ProductName)));

            if (!string.IsNullOrEmpty(filter.Channel))
                query = query.Where(o => o.Channel == filter.Channel);

            if (!string.IsNullOrEmpty(filter.IvrStatus))
                query = query.Where(o => o.IvrStatus == filter.IvrStatus);

            if (!string.IsNullOrEmpty(filter.WhatsAppStatus))
                query = query.Where(o => o.WhatsAppStatus == filter.WhatsAppStatus);

            if (!string.IsNullOrEmpty(filter.Tags))
                query = query.Where(o => o.Tags != null && o.Tags.Contains(filter.Tags));

            if (!string.IsNullOrEmpty(filter.PaymentType))
            {
                if (filter.PaymentType.Equals("COD", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(o => o.PaymentMode == PaymentMode.COD);
                else if (filter.PaymentType.Equals("Prepaid", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(o => o.PaymentMode == PaymentMode.Prepaid);
            }

            if (!string.IsNullOrEmpty(filter.OrderType))
            {
                if (filter.OrderType.Equals("Forward", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(o => o.OrderType == OrderTypeEnum.Forward);
                else if (filter.OrderType.Equals("Reverse", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(o => o.OrderType == OrderTypeEnum.Reverse);
                else if (filter.OrderType.Equals("ReverseQC", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(o => o.OrderType == OrderTypeEnum.ReverseQC);
            }

            return query;
        }


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await Task.CompletedTask;
        }

        public async Task UpdateOrderItem(OrderItem orderitem)
        {
            _context.OrderItems.Update(orderitem);
            await Task.CompletedTask;
        }

        public async Task<Order?> GetForShipmentAsync(long orderId, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
               .AsNoTracking().
               Include(o => o.PickupAddress)
               .Include(o => o.OrderItems).
               FirstOrDefaultAsync( o => o.OrderId == orderId && o.IsActive == true, cancellationToken);
        }
        public async Task<Order?> GetOrderWithItemsAndShipmentAsync(long orderId, string userId)
        {
            return await _context.Orders
                .Include(o => o.PickupAddress) 
                .Include(o => o.OrderItems)
                .Include(o => o.Shipments!)
                    .ThenInclude(s => s.Courier)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.IsActive == true);
        }



    }
}
