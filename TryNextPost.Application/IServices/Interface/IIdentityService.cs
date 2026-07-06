using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Auth;

namespace TryNextPost.Application.Services.Interface
{
    public interface IIdentityService
    {
        Task<IdentityResultModel> CreateUserAsync(string email, string password, string fullName, string mobile);

        Task<ResponseSellerDto> GetSellerById(string id);

        Task<List<ResponseSellerDto>> GetSellerList();

        Task<bool> CheckEmailExistsAsync(string email);
        Task<IdentityResultModel> ValidateCredentialsAsync(string email, string password);
        Task<ResponseSellerDto> GetUserByEmailAsync(string email);

        Task<IdentityResultModel> ResetPasswordAsync(string email, string newPassword);
    }
}
