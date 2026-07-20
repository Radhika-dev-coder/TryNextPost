using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Entities;

namespace TryNextPost.Domain.IRepository
{
    public interface IOtpRepository
    {
        Task AddAsync(Otp otp);
        Task SaveChangesAsync();
        Task InvalidateActiveOtpsAsync(string mobile);
        Task<Otp?> GetLatestActiveByMobileAsync(string mobile);
    }
}
