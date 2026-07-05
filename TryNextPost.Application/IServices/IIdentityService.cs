using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Auth;
using TryNextPost.Domain.Entities;

namespace TryNextPost.Application.IServices
{
    public interface IIdentityService
    {
            Task<IdentityResultModel> CreateUserAsync(string email, string password, string fullName, string mobile);

            Task<SellerDto> GetSellerById(string id);

            Task<List<SellerDto>> GetSellerList();
    }
}
