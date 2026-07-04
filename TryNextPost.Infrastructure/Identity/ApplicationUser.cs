using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TryNextPost.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        // 🔹 Custom Fields
        public string FullName { get; set; } = string.Empty;
        public ICollection<Address>? Addresses { get; set; }

        // 🔹 Business Info (Optional for now ✅)
        public string? CompanyName { get; set; }
        public string? BrandName { get; set; }
        public string? GSTNumber { get; set; }

        // 🔹 Company Relation (🔥 MOST IMPORTANT)
        public string? CompanyId { get; set; }
        public CompanyInfo? Company { get; set; }

        // 🔹 Status
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public bool IsProfileComplete { get; set; } = false;
    }

    public class ApplicaitonRole : IdentityRole
    {
        public string? Description { get; set; }
    }
}
