namespace TryNextPost.Application.DTO.Employee
{
    public class UpdateEmployeeRequest
    {
        public string? FullName { get; set; }
        public bool? IsActive { get; set; }
        public List<string>? Permissions { get; set; }
    }
}
