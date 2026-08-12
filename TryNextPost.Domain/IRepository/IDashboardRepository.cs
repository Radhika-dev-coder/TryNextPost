using TryNextPost.Domain.Entities;

namespace TryNextPost.Domain.IRepository
{
    public interface IDashboardRepository
    {
        Task<int> CountOrdersAsync(long? sellerId, DateTime? from, DateTime? to, bool pendingOnly = false);
        Task<int> CountShipmentsAsync(long? sellerId, DateTime? from, DateTime? to, IEnumerable<int>? statusFilter = null);
        Task<decimal> SumOrderRevenueAsync(long? sellerId, DateTime? from, DateTime? to);
        Task<decimal> SumPlatformRevenueAsync(DateTime? from, DateTime? to);
        Task<int> CountOpenNdrAsync(long? sellerId);
        Task<int> CountRtoAsync(long? sellerId, DateTime? from, DateTime? to);
        Task<decimal?> GetWalletBalanceAsync(long sellerId);
        Task<List<(DateTime Day, int Delivered, int Pending, int Failed)>> GetDailyShipmentBreakdownAsync(
            long sellerId, DateTime from, DateTime to);
        Task<List<(string CourierName, int Total, int Delivered)>> GetCourierPerformanceAsync(
            long sellerId, DateTime from, DateTime to);
        Task<List<Order>> GetRecentOrdersAsync(long? sellerId, int take);
        Task<int> CountSellersAsync(DateTime? from, DateTime? to);
        Task<int> CountActiveCouriersAsync();
        Task<List<(Seller Seller, string FullName, string Email, DateTime Joined)>> GetRecentSellersAsync(int take);
        Task<string?> GetCompanyNameAsync(long companyId);
        Task<int> CountActiveOrdersPlatformAsync(DateTime? from, DateTime? to);
        Task<List<SuperAdminRecentOrderRow>> GetSuperAdminRecentOrdersAsync(int take);
    }

    public class SuperAdminRecentOrderRow
    {
        public string OrderRef { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string SellerDisplayName { get; set; } = string.Empty;
        public string CourierName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
