using TryNextPost.Application.Common;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.Services.Interface;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class
{
    public class SellerContextService : ISellerContextService
    {
        private readonly ISellerRepository _sellerRepository;
        private readonly ISellerEmployeeRepository _employeeRepository;
        private readonly IIdentityService _identityService;

        public SellerContextService(
            ISellerRepository sellerRepository,
            ISellerEmployeeRepository employeeRepository,
            IIdentityService identityService)
        {
            _sellerRepository = sellerRepository;
            _employeeRepository = employeeRepository;
            _identityService = identityService;
        }

        public async Task<SellerContext> ResolveAsync(string userId)
        {
            var seller = await _sellerRepository.GetByUserIdAsync(userId);
            if (seller != null)
            {
                return new SellerContext
                {
                    SellerId = seller.SellerId,
                    UserId = userId,
                    IsOwner = true,
                    Permissions = EmployeePermissionCode.All
                };
            }

            var employee = await _employeeRepository.GetByUserIdAsync(userId);
            if (employee != null)
            {
                var permissions = employee.Permissions?
                    .Select(p => p.PermissionCode)
                    .ToList() ?? new List<string>();

                return new SellerContext
                {
                    SellerId = employee.SellerId,
                    UserId = userId,
                    IsOwner = false,
                    EmployeeId = employee.EmployeeId,
                    Permissions = permissions
                };
            }

            var roles = await _identityService.GetUserRolesAsync(userId);
            if (IsSuperAdmin(roles))
            {
                throw new InvalidOperationException(
                    "SuperAdmin has no seller context. Use platform admin paths instead of seller ResolveAsync.");
            }            
            throw new UnauthorizedAccessException(SystemMessage.SellerNotFound);
        }

        private static bool IsSuperAdmin(IEnumerable<string>? roles)
        {
            if (roles == null)
                return false;
  
            var expected = RoleEnum.SuperAdmin.ToString();
            return roles.Any(r => string.Equals(r, expected, StringComparison.OrdinalIgnoreCase));
        }

        public async Task EnsureOwnerAsync(string userId)
        {
            var context = await ResolveAsync(userId);
            if (!context.IsOwner)
                throw new UnauthorizedAccessException(SystemMessage.Unauthorized);
        }

        public async Task EnsurePermissionAsync(string userId, string permissionCode)
        {
            var context = await ResolveAsync(userId);
            if (!context.HasPermission(permissionCode))
                throw new UnauthorizedAccessException(SystemMessage.Unauthorized);
        }

        public async Task<Seller> ResolveSellerAsync(string userId)
        {
            var context = await ResolveAsync(userId);
            var seller = await _sellerRepository.GetByIdAsync(context.SellerId);
            if (seller == null)
                throw new InvalidOperationException(SystemMessage.SellerNotFound);
            return seller;
        }
    }
}
