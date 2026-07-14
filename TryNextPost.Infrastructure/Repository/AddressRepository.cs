using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class AddressRepository : IAddressRepository
    {
        private readonly AppDbContext _context;
        public AddressRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Address address)
        {
            await _context.Addresses.AddAsync(address);
        }

        public async Task<Address> GetByIdAsync(long addressId)
        {
            return await _context.Addresses.FirstOrDefaultAsync(a => a.AddressId == addressId && a.IsActive == true);
        }

        public async Task<List<Address>> GetByUserIdAsync(string userId, AddressType type)
        {
                 return await _context.Addresses
                .Where(a => a.UserId == userId && a.AddressType == type && a.IsActive == true)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Address address)
        {
            _context.Addresses.Update(address);
            await Task.CompletedTask;
        }
    }
}
