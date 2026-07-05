using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Auth;
using TryNextPost.Application.IServices;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Infrastructure.Identity;

namespace TryNextPost.Infrastructure.Service
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResultModel> CreateUserAsync(string email, string password, string fullName, string mobile)
        {
            var newId = await GenerateNextUserCodeAsync();
            var user = new ApplicationUser
            {
                Id = newId,
                UserName = email,
                Email = email,
                FullName = fullName,
                PhoneNumber = mobile,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);

            if(result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, RoleEnum.Seller.ToString());
            }

            return new IdentityResultModel
            {
                Succeeded = result.Succeeded,
                UserId = user.Id,
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        public async Task<SellerDto> GetSellerById(string id)
        {

            var user = await _userManager.FindByIdAsync(id.Trim());

            if (user == null)
                return null;

            var isSeller = await _userManager.IsInRoleAsync(user, RoleEnum.Seller.ToString());
            if (!isSeller == false) return null;

            return new SellerDto
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
        }

        public async Task<List<SellerDto>> GetSellerList()
        {
            var users = await _userManager.GetUsersInRoleAsync(RoleEnum.Seller.ToString());
           
            return users.Select(user => new SellerDto
            {
                FullName= user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            }).ToList();
        }

        private async Task<string> GenerateNextUserCodeAsync()
        {
            var lastUser = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .FirstOrDefaultAsync();

            long nextNumber = 1;
            if (lastUser != null && lastUser.Id.StartsWith("TNP-"))
            {
                var numberPart = lastUser.Id.Replace("TNP-", "");
                if (long.TryParse(numberPart, out long lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }
            return $"TNP-{nextNumber:D6}";
        }

    }
}
