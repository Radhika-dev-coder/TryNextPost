using TryNextPost.Application.DTO.Admin;

namespace TryNextPost.Application.IServices.Interface.IAdmin
{
    public interface ICourierAdminService
    {
        Task<List<CourierAdminDto>> GetCouriersAsync();
        Task<CourierAdminDto> UpdateCodFeeAsync(long courierId, UpdateCourierCodFeeRequest request, string adminId);
    }
}