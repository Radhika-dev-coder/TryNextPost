using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Auth;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.IServices;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.Identity;

namespace TryNextPost.Infrastructure.Service
{
    public class AuthService : IAuthService
    {
        private readonly IIdentityService _identityService;    
        private readonly ISellerRepository _sellerRepository;   
               

        public AuthService(IIdentityService identityService, ISellerRepository sellerRepository)
        {
            _identityService = identityService;
            _sellerRepository = sellerRepository;
           
        }



        public async Task<ApiResponse<AuthResult>> RegisterAsync(SellerDto dto)
        {
            var identityResult = await _identityService.CreateUserAsync(dto.Email, dto.Password, dto.FullName, dto.PhoneNumber);

            if (!identityResult.Succeeded)
            {
                return new ApiResponse<AuthResult> { Success = false, Message = string.Join(", ", identityResult.Errors) };
            }

            var seller = new Seller
            {
                UserId = identityResult.UserId,     
                //CompanyId = dto.CompanyId,
                //GstNumber = dto.GstNumber,
                Status = SellerStatus.Active,
                CreatedAt = DateTime.UtcNow
                            
            };

             await _sellerRepository.AddSellerAsync(seller);

            return new ApiResponse<AuthResult>
            {
                Success = true,
                Data = new AuthResult { UserId = identityResult.UserId, Email = dto.Email }
            };
        }
    }
}
