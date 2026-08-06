using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Pincode;

namespace TryNextPost.Application.IServices.Interface
{
    public interface IPincodeService
    {
        Task<PincodeResponseDto> GetAddressFromPincode(string pincode);

        Task<LocationResponseDto> GetAddressFromCoordinates(LocationRequestDto request);
    }
}
