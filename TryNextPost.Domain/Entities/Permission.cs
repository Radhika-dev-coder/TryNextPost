using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Entities
{
    public class Permission
    {
        [Key]
        public long PermissionId { get; set; } 
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ICollection<RolePermission>? RolePermissions { get; set; }
    }
}
