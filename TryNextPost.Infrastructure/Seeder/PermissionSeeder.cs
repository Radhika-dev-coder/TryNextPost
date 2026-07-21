using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Seeder
{
    public static class PermissionSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            foreach (var code in EmployeePermissionCode.All)
            {
                var exists = await db.Permissions.AnyAsync(p => p.Name == code);
                if (!exists)
                {
                    await db.Permissions.AddAsync(new Permission
                    {
                        Name = code,
                        Description = code.Replace('.', ' ')
                    });
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
