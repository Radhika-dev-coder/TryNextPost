using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TryNextPost.Application.DTO.Dashboard;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.IServices.Interface.IDashboard;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly ISellerContextService _sellerContextService;
        private readonly ILogger<DashboardService> _logger;

        private static readonly int[] DeliveredStatuses = { (int)ShipmentStatus.Delivered };
        private static readonly int[] FailedStatuses =
        {
         (int)ShipmentStatus.RTOInitiated,
         (int)ShipmentStatus.RTOInTransit,
         (int)ShipmentStatus.RTODelivered,
         (int)ShipmentStatus.RTOAcknowledged,
         (int)ShipmentStatus.Exception,
         (int)ShipmentStatus.Cancelled,
         (int)ShipmentStatus.BookingFailed
        };

        public DashboardService(
            IDashboardRepository dashboardRepository,
            ISellerContextService sellerContextService,
            ILogger<DashboardService> logger)
        {
            _dashboardRepository = dashboardRepository;
            _sellerContextService = sellerContextService;
            _logger = logger;
        }

        public async Task<SellerDashboardResponse> GetSellerDashboardAsync(string userId)
        {
            var seller = await _sellerContextService.ResolveSellerAsync(userId);
            var (periodStart, periodEnd, prevStart, prevEnd) = GetPeriods();
            var sw = Stopwatch.StartNew();

            // Current-period stats (single pass — reused for StatChanges; DbContext is not thread-safe).
            var totalOrders = await _dashboardRepository.CountOrdersAsync(seller.SellerId, periodStart, periodEnd);
            var totalShipments = await _dashboardRepository.CountShipmentsAsync(seller.SellerId, periodStart, periodEnd);
            var successful = await _dashboardRepository.CountShipmentsAsync(
                seller.SellerId, periodStart, periodEnd, DeliveredStatuses);
            var failed = await _dashboardRepository.CountShipmentsAsync(
                seller.SellerId, periodStart, periodEnd, FailedStatuses);
            var pendingOrders = await _dashboardRepository.CountOrdersAsync(seller.SellerId, null, null, pendingOnly: true);
            var pendingInPeriod = await _dashboardRepository.CountOrdersAsync(
                seller.SellerId, periodStart, periodEnd, pendingOnly: true);
            var revenue = await _dashboardRepository.SumOrderRevenueAsync(seller.SellerId, periodStart, periodEnd);
            var ndr = await _dashboardRepository.CountOpenNdrAsync(seller.SellerId);
            var rto = await _dashboardRepository.CountRtoAsync(seller.SellerId, periodStart, periodEnd);
            failed = Math.Max(failed, rto);
            var wallet = await _dashboardRepository.GetWalletBalanceAsync(seller.SellerId) ?? 0m;

            // Previous period only for deltas (avoid re-querying current period).
            var prevOrders = await _dashboardRepository.CountOrdersAsync(seller.SellerId, prevStart, prevEnd);
            var prevShipments = await _dashboardRepository.CountShipmentsAsync(seller.SellerId, prevStart, prevEnd);
            var prevSuccessful = await _dashboardRepository.CountShipmentsAsync(
                seller.SellerId, prevStart, prevEnd, DeliveredStatuses);
            var prevPending = await _dashboardRepository.CountOrdersAsync(
                seller.SellerId, prevStart, prevEnd, pendingOnly: true);
            var prevFailed = await _dashboardRepository.CountShipmentsAsync(
                seller.SellerId, prevStart, prevEnd, FailedStatuses);

            var weekly = await BuildWeeklyOverviewAsync(seller.SellerId, periodStart, periodEnd);
            var courier = await BuildCourierPerformanceAsync(seller.SellerId, periodStart, periodEnd);
            var recent = await _dashboardRepository.GetRecentOrdersAsync(seller.SellerId, 5);

            _logger.LogInformation(
                "Dashboard GetSellerDashboardAsync seller={SellerId} ms={Ms}",
                seller.SellerId, sw.ElapsedMilliseconds);

            return new SellerDashboardResponse
            {
                Stats = new SellerDashboardStatsDto
                {
                    TotalOrders = totalOrders,
                    TotalShipments = totalShipments,
                    SuccessfulDeliveries = successful,
                    FailedDeliveries = failed,
                    PendingOrders = pendingOrders,
                    TotalRevenue = revenue,
                    NdrCount = ndr,
                    RtoCount = rto,
                    WalletBalance = wallet
                },
                StatChanges =
                [
                    BuildChange("totalOrders", totalOrders, prevOrders),
                    BuildChange("totalShipments", totalShipments, prevShipments),
                    BuildChange("successfulDeliveries", successful, prevSuccessful),
                    BuildChange("pendingOrders", pendingInPeriod, prevPending),
                    BuildChange("failedDeliveries", failed, prevFailed)
                ],
                WeeklyOverview = weekly,
                CourierPerformance = courier,
                RecentOrders = recent.Select(MapRecentOrder).ToList()
            };
        }

        public async Task<SuperAdminDashboardResponse> GetSuperAdminDashboardAsync()
        {
            var (periodStart, periodEnd, prevStart, prevEnd) = GetPeriods();
            var sw = Stopwatch.StartNew();

            var totalUsers = await _dashboardRepository.CountSellersAsync(null, null);
            var activeOrders = await _dashboardRepository.CountActiveOrdersPlatformAsync(null, null);
            var revenue = await _dashboardRepository.SumPlatformRevenueAsync(periodStart, periodEnd);
            var couriers = await _dashboardRepository.CountActiveCouriersAsync();
            var ndr = await _dashboardRepository.CountOpenNdrAsync(null);

            var prevUsers = await _dashboardRepository.CountSellersAsync(prevStart, prevEnd);
            var prevOrders = await _dashboardRepository.CountActiveOrdersPlatformAsync(prevStart, prevEnd);
            var prevRevenue = await _dashboardRepository.SumPlatformRevenueAsync(prevStart, prevEnd);
            // Current-period user/order deltas need current-window counts (not all-time totals above).
            var curUsersWindow = await _dashboardRepository.CountSellersAsync(periodStart, periodEnd);
            var curOrdersWindow = await _dashboardRepository.CountActiveOrdersPlatformAsync(periodStart, periodEnd);

            var recentUsers = await BuildRecentUsersAsync();
            var recentOrders = await _dashboardRepository.GetSuperAdminRecentOrdersAsync(5);

            _logger.LogInformation("Dashboard GetSuperAdminDashboardAsync ms={Ms}", sw.ElapsedMilliseconds);

            return new SuperAdminDashboardResponse
            {
                Stats = new SuperAdminDashboardStatsDto
                {
                    TotalUsers = totalUsers,
                    ActiveOrders = activeOrders,
                    TotalRevenue = revenue,
                    SupportTickets = 0,
                    CourierPartners = couriers,
                    OpenNdrCount = ndr
                },
                StatChanges =
                [
                    BuildChange("totalUsers", curUsersWindow, prevUsers),
                    BuildChange("activeOrders", curOrdersWindow, prevOrders),
                    BuildChange("totalRevenue", revenue, prevRevenue),
                    BuildChange("supportTickets", 0m, 0m),
                    BuildChange("courierPartners", couriers, couriers)
                ],
                RecentUsers = recentUsers,
                RecentOrders = recentOrders
                    .Select(o => new SuperAdminRecentOrderDto
                    {
                        Id = o.OrderRef,
                        User = o.SellerDisplayName,
                        Amount = o.Amount,
                        Courier = o.CourierName,
                        Status = o.Status
                    })
                    .ToList()
            };
        }

        private async Task<List<WeeklyShipmentBarDto>> BuildWeeklyOverviewAsync(
            long sellerId,
            DateTime periodStart,
            DateTime periodEnd)
        {
            var daily = await _dashboardRepository.GetDailyShipmentBreakdownAsync(sellerId, periodStart, periodEnd);
            var lookup = daily.ToDictionary(d => d.Day.Date);

            var bars = new List<WeeklyShipmentBarDto>();
            for (var day = periodStart.Date; day < periodEnd.Date; day = day.AddDays(1))
            {
                lookup.TryGetValue(day, out var row);
                var total = row.Delivered + row.Pending + row.Failed;
                bars.Add(new WeeklyShipmentBarDto
                {
                    Label = day.ToString("ddd"),
                    Delivered = ToBarPercent(row.Delivered, total),
                    Pending = ToBarPercent(row.Pending, total),
                    Failed = ToBarPercent(row.Failed, total)
                });
            }

            return bars;
        }

        private async Task<List<CourierPerformanceDto>> BuildCourierPerformanceAsync(
            long sellerId,
            DateTime periodStart,
            DateTime periodEnd)
        {
            var rows = await _dashboardRepository.GetCourierPerformanceAsync(sellerId, periodStart, periodEnd);
            return rows.Select(r => new CourierPerformanceDto
            {
                Name = r.CourierName,
                Shipments = r.Total,
                SuccessRate = r.Total == 0 ? 0 : Math.Round((decimal)r.Delivered / r.Total * 100m, 1)
            }).ToList();
        }

        private async Task<List<SuperAdminRecentUserDto>> BuildRecentUsersAsync()
        {
            var rows = await _dashboardRepository.GetRecentSellersAsync(5);
            var result = new List<SuperAdminRecentUserDto>(rows.Count);

            foreach (var row in rows)
            {
                var companyName = row.Seller.CompanyId.HasValue
                    ? await _dashboardRepository.GetCompanyNameAsync(row.Seller.CompanyId.Value)
                    : null;

                result.Add(new SuperAdminRecentUserDto
                {
                    Id = row.Seller.UserId,
                    Name = row.FullName,
                    Email = row.Email,
                    Company = companyName ?? row.FullName,
                    Plan = string.Empty,
                    Status = MapSellerStatus(row.Seller.Status),
                    Joined = row.Joined
                });
            }

            return result;
        }

        private static (DateTime PeriodStart, DateTime PeriodEnd, DateTime PrevStart, DateTime PrevEnd) GetPeriods()
        {
            var periodEnd = DateTime.UtcNow.Date.AddDays(1);
            var periodStart = periodEnd.AddDays(-7);
            var prevEnd = periodStart;
            var prevStart = prevEnd.AddDays(-7);
            return (periodStart, periodEnd, prevStart, prevEnd);
        }

        private static DashboardStatChangeDto BuildChange(string key, decimal current, decimal previous)
        {
            decimal percent;
            if (previous == 0)
                percent = current > 0 ? 100 : 0;
            else
                percent = Math.Round((current - previous) / previous * 100m, 1);

            return new DashboardStatChangeDto
            {
                Key = key,
                ChangePercent = Math.Abs(percent),
                Direction = percent >= 0 ? "up" : "down"
            };
        }

        private static int ToBarPercent(int value, int total) =>
            total == 0 ? 0 : (int)Math.Round((decimal)value / total * 100m);

        private static DashboardRecentOrderDto MapRecentOrder(Domain.Entities.Order order) => new()
        {
            Id = order.OrderId.ToString(),
            OrderId = order.OrderRef,
            Customer = order.CustomerName,
            Status = MapOrderStatus(order.Status),
            Amount = order.FinalPayableAmount,
            CreatedDate = order.CreatedAt ?? order.OrderDate,
            DeliveryDate = order.Status == OrderStatus.Delivered ? order.UpdatedAt : null
        };

        private static string MapOrderStatus(OrderStatus status) => status switch
        {
            OrderStatus.Pending => "pending",
            OrderStatus.Confirmed or OrderStatus.Packed => "processing",
            OrderStatus.Shipped => "shipped",
            OrderStatus.Delivered => "delivered",
            OrderStatus.Cancelled => "cancelled",
            OrderStatus.RTO => "failed",
            _ => "pending"
        };

        private static string MapSellerStatus(SellerStatus status) => status switch
        {
            SellerStatus.Active => "active",
            SellerStatus.PendingVerification => "pending",
            SellerStatus.Suspended => "suspended",
            SellerStatus.Inactive => "suspended",
            _ => "pending"
        };
    }
}
