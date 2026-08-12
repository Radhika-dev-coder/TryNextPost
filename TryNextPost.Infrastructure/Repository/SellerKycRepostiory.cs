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

        public async Task AddAsync(SellerKYCDetails sellerKyc)
        {
            await _context.SellerKYC.AddAsync(sellerKyc);
            await _context.SaveChangesAsync();
        }
        public async Task AddPanKycAsync(PANKYC pan)
        {
            await _context.PANKYCs.AddAsync(pan);
            await _context.SaveChangesAsync();
        }
        public async Task AddBankKycAsync(BankKYC kyc)
        {
            await _context.BankKYCs.AddAsync(kyc);
            await _context.SaveChangesAsync();
        }

        public async Task<SellerKYCDetails?> GetBySellerIdAsync(string sellerId)
        {
            return await _context.SellerKYC.FirstOrDefaultAsync(x => x.SellerId == sellerId && x.IsActive== true);
        }
        public async Task<PANKYC?> GetByPanSellerKYCAsync(string sellerId)
        {
            return await _context.PANKYCs.FirstOrDefaultAsync(x => x.SellerKyc.SellerId == sellerId && x.IsActive== true);
        }
        
        public async Task<BankKYC?> GetByBankSellerKYCAsync(string sellerId)
        {
            return await _context.BankKYCs.FirstOrDefaultAsync(x => x.SellerKyc.SellerId == sellerId && x.IsActive== true);
        }
        

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public  Task UpdateAsync(SellerKYCDetails sellerKyc)
        {
            _context.SellerKYC.Update(sellerKyc);
            _context.SaveChanges();
            return Task.CompletedTask;
        }
        public  Task UpdatePanKycAsync(PANKYC pan)
        {
            _context.PANKYCs.Update(pan);
            _context.SaveChanges();
            return Task.CompletedTask;
        }
        public  Task UpdateBankKycAsync(BankKYC kyc)
        {
            _context.BankKYCs.Update(kyc);
            _context.SaveChanges();
            return Task.CompletedTask;
        }
    }
}
