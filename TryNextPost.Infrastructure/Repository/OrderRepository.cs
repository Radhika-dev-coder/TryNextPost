using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.IRepository;
using Microsoft.EntityFrameworkCore;
using TryNextPost.Infrastructure.AppDbContexts;


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

        public async Task<Order> GetByIdAsync(long orderId)
        {
            return await _context.Orders.Include(o => o.OrderItems).
                FirstOrDefaultAsync(o => o.OrderId == orderId && o.IsActive == true);
        }

        public async Task<List<Order>> GetBySellerIdAsync(long sellerId)
        {
            return await _context.Orders.Include(o => o.OrderItems).
                Where(o => o.SellerId == sellerId && o.IsActive == true).ToListAsync();
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
    }
}
