using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;

namespace TryNextPost.Domain.IRepository
{
    public interface ISellerKycRepository
    {
        Task<SellerKYC?> GetBySellerIdAsync(string sellerId);

        Task AddAsync(SellerKYC sellerKyc);

        Task UpdateAsync(SellerKYC sellerKyc);

        Task<bool> SaveChangesAsync();
    }
}
