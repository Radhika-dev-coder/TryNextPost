using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Default;
using TryNextPost.Application.IServices.Interface.Default;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class.Default
{
    public class AddressService : IAddressService
    {
        private readonly ISellerRepository _sellerRepository;
        private readonly IAddressRepository _addressRepository;

        public AddressService(ISellerRepository sellerRepository, IAddressRepository addressRepository)
        { 
            _sellerRepository = sellerRepository;
            _addressRepository = addressRepository;
        }
        public async Task<long> AddPickupAddressAsync(AddPickupAddressRequest request, string userId)
        {
            var seller = await _sellerRepository.GetByUserIdAsync(userId);
            if (seller == null)
                throw new InvalidOperationException(string.Format(SystemMessage.SellerNotFound));

            var address = new Address
            {
                AddressType = AddressType.SellerPickup,
                UserId = userId,
                CompanyId = seller.CompanyId,  

                WarehouseName = request.WarehouseName,
                Name = request.ContactName,
                Email = request.Email,
                Mobile = request.ContactNumber,
                GstNumber = request.GstNumber,

                AddressLine1 = request.AddressDetails,
                City = request.City,
                State = request.State,
                Pincode = request.Pincode,
                Country = "India",

                IsActive = true,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _addressRepository.AddAsync(address);
            await _addressRepository.SaveChangesAsync();

            // First active warehouse becomes the seller default return/pickup warehouse
            // (needed for Reverse / ReverseQC rates when order.PickupAddressId is null).
            if (!seller.DefaultPickupAddressId.HasValue)
            {
                seller.DefaultPickupAddressId = address.AddressId;
                seller.UpdatedAt = DateTime.UtcNow;
                seller.UpdatedBy = userId;
                await _sellerRepository.UpdateAsync(seller);
                await _sellerRepository.SaveChangesAsync();
            }

            return address.AddressId;
        }

        public async Task DeletePickupAddressAsync(long addressId, string userId)
        {
           var address = await _addressRepository.GetByIdAsync(addressId);
            if (address == null) throw new InvalidOperationException(string.Format(SystemMessage.AddressNotFound));

            if(address.UserId != userId)
                throw new UnauthorizedAccessException(string.Format(SystemMessage.Unauthorized));

            address.IsActive = false;
            address.UpdatedAt = DateTime.UtcNow;
            address.UpdatedBy = userId;

            await _addressRepository.UpdateAsync(address);
            await _addressRepository.SaveChangesAsync();

        }

        public async Task<AddressResponse> GetPickupAddressByIdAsync(long addressId, string userId)
        {
            var address = await _addressRepository.GetByIdAsync(addressId);
            if (address == null)
                throw new InvalidOperationException(string.Format(SystemMessage.AddressNotFound));

            
            if (address.UserId != userId)
                throw new UnauthorizedAccessException(string.Format(SystemMessage.Unauthorized));

            return new AddressResponse
            {
                AddressId = address.AddressId,
                WarehouseName = address.WarehouseName,
                Name = address.Name,
                Email = address.Email,
                Mobile = address.Mobile,
                GstNumber = address.GstNumber,
                Pincode = address.Pincode,
                City = address.City,
                State = address.State,
                AddressLine1 = address.AddressLine1,
                Country = address.Country
            };
        }

        public async Task<List<AddressResponse>> GetPickupAddressesAsync(string userId)
        {
            var addresses = await _addressRepository.GetByUserIdAsync(userId, AddressType.SellerPickup);

            return addresses.Select(a => new AddressResponse
            {
                AddressId = a.AddressId,
                WarehouseName = a.WarehouseName,
                Name = a.Name,
                Email = a.Email,
                Mobile = a.Mobile,
                GstNumber = a.GstNumber,
                Pincode = a.Pincode,
                City = a.City,
                State = a.State,
                AddressLine1 = a.AddressLine1,
                Country = a.Country
            }).ToList();
        }

        public async Task UpdatePickupAddressAsync(long addressId, UpdatePickupAddressRequest request, string userId)
        {
            var address = await _addressRepository.GetByIdAsync(addressId);
            if (address == null)
                throw new InvalidOperationException(string.Format(SystemMessage.AddressNotFound));

            if (address.UserId != userId)
                throw new UnauthorizedAccessException(string.Format(SystemMessage.Unauthorized));

         
            address.WarehouseName = request.WarehouseName;
            address.Name = request.ContactName;
            address.Email = request.Email;
            address.Mobile = request.ContactNumber;
            address.GstNumber = request.GstNumber;
            address.AddressLine1 = request.AddressDetails;
            address.City = request.City;
            address.State = request.State;
            address.Pincode = request.Pincode;
            address.UpdatedAt = DateTime.UtcNow;
            address.UpdatedBy = userId;

            await _addressRepository.UpdateAsync(address);
            await _addressRepository.SaveChangesAsync();

        }
    }
    
}
