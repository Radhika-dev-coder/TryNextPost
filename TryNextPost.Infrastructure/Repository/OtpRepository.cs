using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;
using Microsoft.EntityFrameworkCore;

namespace TryNextPost.Infrastructure.Repository
{
    public class OtpRepository : IOtpRepository
    {
        private readonly AppDbContext _context;

        public OtpRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(Otp otp)
        {
            await _context.Otps.AddAsync(otp);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task InvalidateActiveOtpsAsync(string mobile)
        {
            var actives = await _context.Otps
           .Where(x => x.MobileNumber == mobile && x.IsUsed == false)
           .ToListAsync();
            foreach (var item in actives)
                item.IsUsed = true;
        }

        public async Task<Otp?> GetLatestActiveByMobileAsync(string mobile)
        {
            return await _context.Otps
           .Where(x => x.MobileNumber == mobile
                 && x.IsUsed == false
                 && x.ExpiryTime >= DateTime.UtcNow)
              .OrderByDescending(x => x.CreatedAt)
              .FirstOrDefaultAsync();
        }
    }
}
