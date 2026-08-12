using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Entities;

namespace TryNextPost.Domain.IRepository
{
    public interface ISellerRepository
    {
        Task AddSellerAsync(Seller seller);
        Task<Seller> GetByUserIdAsync(string  userId);

        Task<Seller?> GetByIdAsync(long sellerId);

        /// <summary>Active sellers with company name for SuperAdmin pickers.</summary>
        Task<List<Seller>> GetActiveLookupAsync();

        Task CreateSellerAsync(string UserId);

        Task UpdateAsync(Seller seller);
        Task SaveChangesAsync();
    }
}
