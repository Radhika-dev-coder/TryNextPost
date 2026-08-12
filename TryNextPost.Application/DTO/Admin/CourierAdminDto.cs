using TryNextPost.Domain.Enums;

namespace TryNextPost.Application.DTO.Admin
{
    public class CourierAdminDto
    {
        public long CourierId { get; set; }
        public string CourierName { get; set; } = string.Empty;
        public string CourierCode { get; set; } = string.Empty;
        public bool SupportsCOD { get; set; }
        public bool SupportsPrepaid { get; set; }
        public bool IsActive { get; set; }
        public CodChargeType CodChargeType { get; set; }
        public decimal CodChargeValue { get; set; }
        public string CodLabel { get; set; } = string.Empty;
    }
}