using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Entities
{
    public class RolePermission
    {
        [Key]
        public long Id { get; set; } 
        public string RoleId { get; set; } = string.Empty;
        public long PermissionId { get; set; } 
        public Permission? Permission { get; set; }
    }
}
