using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Default;

namespace TryNextPost.Application.IServices.Interface.Default
{
    public interface IAddressService
    {
        Task<long> AddPickupAddressAsync(AddPickupAddressRequest request, string userId);
        Task<List<AddressResponse>> GetPickupAddressesAsync(string userId);

        Task<AddressResponse> GetPickupAddressByIdAsync(long addressId, string userId);
        Task UpdatePickupAddressAsync(long addressId, UpdatePickupAddressRequest request, string userId);
        Task DeletePickupAddressAsync(long addressId, string userId);

    }
}
