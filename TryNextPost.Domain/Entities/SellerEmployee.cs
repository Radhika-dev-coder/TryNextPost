using System.ComponentModel.DataAnnotations;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class SellerEmployee : BaseDbModel
    {
        [Key]
        public long EmployeeId { get; set; }

        public long SellerId { get; set; }
        public Seller? Seller { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public ICollection<EmployeePermission>? Permissions { get; set; }
    }
}
