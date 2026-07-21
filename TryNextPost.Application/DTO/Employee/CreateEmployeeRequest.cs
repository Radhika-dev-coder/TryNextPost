namespace TryNextPost.Application.DTO.Employee
{
    public class CreateEmployeeRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public List<string> Permissions { get; set; } = new();
    }
}
