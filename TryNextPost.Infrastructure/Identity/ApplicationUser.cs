using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.Entities;


namespace TryNextPost.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public ICollection<Address>? Addresses { get; set; }


        public Seller? Seller { get; set; }

        public string? BrandName { get; set; }   

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsProfileComplete { get; set; } = false;
    }

    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }

        public ICollection<RolePermission>? RolePermissions { get; set; }
    }
}
