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

    }
}
