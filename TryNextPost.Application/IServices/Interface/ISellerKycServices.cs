using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO;
using TryNextPost.Domain.Common;

namespace TryNextPost.Application.IServices.Interface
{
    public interface ISellerKycServices
    {
        Task<BaseResponse<object>> SendOtpAadharKyc(SendAadhaarOtpRequestDto dto,string sellerId);
        Task<BaseResponse<object>> AddSellerKycAsync(VerifyAadhaarOtpRequestDto dto, string sellerId);
    }
}
