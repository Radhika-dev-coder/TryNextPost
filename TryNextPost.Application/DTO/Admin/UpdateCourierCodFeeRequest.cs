using TryNextPost.Domain.Enums;

namespace TryNextPost.Application.DTO.Admin
{
    public class UpdateCourierCodFeeRequest
    {
        public CodChargeType CodChargeType { get; set; }
        public decimal CodChargeValue { get; set; }
    }
}