using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class SellerRepository : ISellerRepository
    {
        private readonly AppDbContext _appDbContexts;

        public SellerRepository(AppDbContext appDbContexts)
        {
            _appDbContexts = appDbContexts;
        }

        public async Task AddSellerAsync(Seller seller)
        {
           await _appDbContexts.Sellers.AddAsync(seller);
           await _appDbContexts.SaveChangesAsync();
        }

        public async Task CreateSellerAsync(string UserId)
        {
            var seller = new Seller
            { 
                UserId = UserId,
                Status = Domain.Enums.SellerStatus.Active,
                IsActive = true,
                CreatedBy = "Admin",
                CreatedAt = DateTime.UtcNow,
            };
            await _appDbContexts.Sellers.AddAsync(seller);
            await _appDbContexts.SaveChangesAsync();
        }

        public async Task<Seller> GetByUserIdAsync(string userId)
        {
            return await _appDbContexts.Sellers.FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public Task UpdateAsync(Seller seller)
        {
            _appDbContexts.Sellers.Update(seller);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _appDbContexts.SaveChangesAsync();
        }
    }
}
