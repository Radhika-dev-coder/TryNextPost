using TryNextPost.Application.Common;
using TryNextPost.Domain.Entities;

namespace TryNextPost.Application.IServices.Interface
{
    public interface ISellerContextService
    {
        Task<SellerContext> ResolveAsync(string userId);
        Task<Seller> ResolveSellerAsync(string userId);
        Task EnsureOwnerAsync(string userId);
        Task EnsurePermissionAsync(string userId, string permissionCode);
    }
}
