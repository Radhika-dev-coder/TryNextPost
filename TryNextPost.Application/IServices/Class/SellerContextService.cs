using TryNextPost.Application.Common;
using TryNextPost.Application.IServices.Interface;
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

        public SellerContextService(
            ISellerRepository sellerRepository,
            ISellerEmployeeRepository employeeRepository)
        {
            _sellerRepository = sellerRepository;
            _employeeRepository = employeeRepository;
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
            if (employee == null)
                throw new UnauthorizedAccessException(SystemMessage.SellerNotFound);

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
