namespace TryNextPost.Application.DTO.Employee
{
    public class EmployeeResponse
    {
        public long EmployeeId { get; set; }
        public long SellerId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<string> Permissions { get; set; } = new();
        public DateTime? CreatedAt { get; set; }
    }
}
