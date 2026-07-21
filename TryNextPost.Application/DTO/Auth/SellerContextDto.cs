namespace TryNextPost.Application.DTO.Auth
{
    public class SellerContextDto
    {
        public long SellerId { get; set; }
        public bool IsOwner { get; set; }
        public long? EmployeeId { get; set; }
        public List<string> Permissions { get; set; } = new();
    }
}
