using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class SellerKycRepostiory : ISellerKycRepository
    {
        private readonly AppDbContext _context;
        public SellerKycRepostiory(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(SellerKYC sellerKyc)
        {
            await _context.SellerKYC.AddAsync(sellerKyc);
        }

        public async Task<SellerKYC?> GetBySellerIdAsync(string sellerId)
        {
            return await _context.SellerKYC.FirstOrDefaultAsync(x => x.SellerId == sellerId && x.IsActive== true);
        }
        

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public  Task UpdateAsync(SellerKYC sellerKyc)
        {
            _context.SellerKYC.Update(sellerKyc);
            return Task.CompletedTask;
        }
    }
}
