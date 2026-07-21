using TryNextPost.Domain.Entities;

namespace TryNextPost.Domain.IRepository
{
    public interface ISellerEmployeeRepository
    {
        Task<SellerEmployee?> GetByUserIdAsync(string userId);
        Task<SellerEmployee?> GetByIdAsync(long employeeId);
        Task<SellerEmployee?> GetByIdAndSellerIdAsync(long employeeId, long sellerId);
        Task<List<SellerEmployee>> GetBySellerIdAsync(long sellerId);
        Task AddAsync(SellerEmployee employee);
        Task UpdateAsync(SellerEmployee employee);
        Task ReplacePermissionsAsync(long employeeId, IEnumerable<string> permissionCodes);
        Task<List<string>> GetPermissionCodesAsync(long employeeId);
        Task SaveChangesAsync();
    }
}
