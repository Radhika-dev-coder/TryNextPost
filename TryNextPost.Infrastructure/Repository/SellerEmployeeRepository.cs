using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class SellerEmployeeRepository : ISellerEmployeeRepository
    {
        private readonly AppDbContext _context;

        public SellerEmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SellerEmployee?> GetByUserIdAsync(string userId)
        {
            return await _context.SellerEmployees
                .Include(e => e.Permissions)
                .FirstOrDefaultAsync(e => e.UserId == userId && e.IsActive == true);
        }

        public async Task<SellerEmployee?> GetByIdAsync(long employeeId)
        {
            return await _context.SellerEmployees
                .Include(e => e.Permissions)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<SellerEmployee?> GetByIdAndSellerIdAsync(long employeeId, long sellerId)
        {
            return await _context.SellerEmployees
                .Include(e => e.Permissions)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.SellerId == sellerId);
        }

        public async Task<List<SellerEmployee>> GetBySellerIdAsync(long sellerId)
        {
            return await _context.SellerEmployees
                .Include(e => e.Permissions)
                .Where(e => e.SellerId == sellerId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(SellerEmployee employee)
        {
            await _context.SellerEmployees.AddAsync(employee);
        }

        public Task UpdateAsync(SellerEmployee employee)
        {
            _context.SellerEmployees.Update(employee);
            return Task.CompletedTask;
        }

        public async Task ReplacePermissionsAsync(long employeeId, IEnumerable<string> permissionCodes)
        {
            var existing = await _context.EmployeePermissions
                .Where(p => p.EmployeeId == employeeId)
                .ToListAsync();

            _context.EmployeePermissions.RemoveRange(existing);

            foreach (var code in permissionCodes.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                await _context.EmployeePermissions.AddAsync(new EmployeePermission
                {
                    EmployeeId = employeeId,
                    PermissionCode = code
                });
            }
        }

        public async Task<List<string>> GetPermissionCodesAsync(long employeeId)
        {
            return await _context.EmployeePermissions
                .Where(p => p.EmployeeId == employeeId)
                .Select(p => p.PermissionCode)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
