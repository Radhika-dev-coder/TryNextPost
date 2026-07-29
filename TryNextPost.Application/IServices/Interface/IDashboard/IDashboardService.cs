using TryNextPost.Application.DTO.Dashboard;

namespace TryNextPost.Application.IServices.Interface.IDashboard
{
    public interface IDashboardService
    {
        Task<SellerDashboardResponse> GetSellerDashboardAsync(string userId);
        Task<SuperAdminDashboardResponse> GetSuperAdminDashboardAsync();
    }
}
