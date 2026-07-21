namespace TryNextPost.Application.Common
{
    public class SellerContext
    {
        public long SellerId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public bool IsOwner { get; set; }
        public long? EmployeeId { get; set; }
        public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();

        public bool HasPermission(string permissionCode) =>
            IsOwner || Permissions.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);
    }
}
