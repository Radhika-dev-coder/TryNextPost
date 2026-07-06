using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Auth;
using TryNextPost.Application.Services.Interface;
using TryNextPost.Domain.Enums;
using TryNextPost.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace TryNextPost.Infrastructure.Service
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public IdentityService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }

        public async Task<IdentityResultModel> CreateUserAsync(string email, string password, string fullName, string mobile)
        {
            var existingEmailUser = await _userManager.FindByEmailAsync(email);
            if (existingEmailUser != null)
            {
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<string> { "Email Already Registered" }
                };
            }

            var existingMobileUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == mobile);
            if (existingMobileUser != null)
            {
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<string> { "Mobile Number Already Registered" }
                };
            }

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

            if (result.Succeeded)
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

        public async Task<ResponseSellerDto> GetSellerById(string id)
        {

            var user = await _userManager.FindByIdAsync(id.Trim());

            if (user == null)
                return null;

            var isSeller = await _userManager.IsInRoleAsync(user, RoleEnum.Seller.ToString());
            if (!isSeller) return null;

            return new ResponseSellerDto
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
        }

        public async Task<List<ResponseSellerDto>> GetSellerList()
        {
            var users = await _userManager.GetUsersInRoleAsync(RoleEnum.Seller.ToString());

            return users.Select(user => new ResponseSellerDto
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            }).ToList();
        }

        public async Task<ResponseSellerDto> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            return new ResponseSellerDto
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber
            };
        }

        public async Task<IdentityResultModel> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<string> { "User not found" }
                };

            
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

            return new IdentityResultModel
            {
                Succeeded = result.Succeeded,
                UserId = user.Id,
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        public async Task<IdentityResultModel> ValidateCredentialsAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return new IdentityResultModel { Succeeded = false, Errors = new List<string> { "Invalid email or password" } };

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);

            if (!result.Succeeded)
                return new IdentityResultModel { Succeeded = false, Errors = new List<string> { "Invalid email or password" } };

            return new IdentityResultModel { Succeeded = true, UserId = user.Id };
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
