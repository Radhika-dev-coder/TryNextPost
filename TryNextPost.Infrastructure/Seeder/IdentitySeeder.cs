using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Enums;
using TryNextPost.Infrastructure.Identity;

namespace TryNextPost.Infrastructure.Seeder
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager,RoleManager<ApplicationRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(RoleEnum.SuperAdmin.ToString()))
                await roleManager.CreateAsync(new ApplicationRole { Name=RoleEnum.SuperAdmin.ToString()});
       
            if (!await roleManager.RoleExistsAsync(RoleEnum.Admin.ToString()))
                await roleManager.CreateAsync(new ApplicationRole { Name=RoleEnum.Admin.ToString()});
       
            if (!await roleManager.RoleExistsAsync(RoleEnum.Seller.ToString()))
                await roleManager.CreateAsync(new ApplicationRole { Name=RoleEnum.Seller.ToString()});

            if (!await roleManager.RoleExistsAsync(RoleEnum.SellerEmployee.ToString()))
                await roleManager.CreateAsync(new ApplicationRole { Name = RoleEnum.SellerEmployee.ToString() });

            var superAdmin = await userManager.FindByEmailAsync("SuperAdmin@yopmail.com");
            if(superAdmin == null)
            {
                superAdmin = new ApplicationUser
                {
                    UserName = "SuperAdmin@yopmail.com",
                    Email = "SuperAdmin@yopmail.com",
                    FullName = "Super Admin",
                    EmailConfirmed = true,
                    IsProfileComplete = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await userManager.CreateAsync(superAdmin,"SuperAdmin@123");
                await userManager.AddToRoleAsync(superAdmin,RoleEnum.SuperAdmin.ToString());
            }       
        }
    }
}
