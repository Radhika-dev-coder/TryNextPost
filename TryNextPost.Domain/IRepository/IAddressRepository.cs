using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.IRepository
{
    public interface IAddressRepository
    {
        Task AddAsync(Address address);
        Task<Address?> GetByIdAsync(long addressId, CancellationToken cancellationToken = default);
        Task<List<Address>> GetByUserIdAsync(string userId, AddressType type);
        Task UpdateAsync(Address address);
        Task SaveChangesAsync();

        Task<bool> IsPickupAddressValidAsync(long addressId, string UserId);
    }
}
