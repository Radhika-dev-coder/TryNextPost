using System.ComponentModel.DataAnnotations;

namespace TryNextPost.Domain.Entities
{
    public class EmployeePermission
    {
        [Key]
        public long Id { get; set; }

        public long EmployeeId { get; set; }
        public SellerEmployee? Employee { get; set; }

        public string PermissionCode { get; set; } = string.Empty;
    }
}
