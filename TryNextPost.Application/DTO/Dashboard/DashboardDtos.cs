namespace TryNextPost.Application.DTO.Dashboard
{
    public class SellerDashboardResponse
    {
        public SellerDashboardStatsDto Stats { get; set; } = new();
        public List<DashboardStatChangeDto> StatChanges { get; set; } = new();
        public List<WeeklyShipmentBarDto> WeeklyOverview { get; set; } = new();
        public List<CourierPerformanceDto> CourierPerformance { get; set; } = new();
        public List<DashboardRecentOrderDto> RecentOrders { get; set; } = new();
    }

    public class SuperAdminDashboardResponse
    {
        public SuperAdminDashboardStatsDto Stats { get; set; } = new();
        public List<DashboardStatChangeDto> StatChanges { get; set; } = new();
        public List<SuperAdminRecentUserDto> RecentUsers { get; set; } = new();
        public List<SuperAdminRecentOrderDto> RecentOrders { get; set; } = new();
    }

    public class SellerDashboardStatsDto
    {
        public int TotalOrders { get; set; }
        public int TotalShipments { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int NdrCount { get; set; }
        public int RtoCount { get; set; }
        public decimal WalletBalance { get; set; }
    }

    public class SuperAdminDashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int SupportTickets { get; set; }
        public int CourierPartners { get; set; }
        public int OpenNdrCount { get; set; }
    }

    public class DashboardStatChangeDto
    {
        public string Key { get; set; } = string.Empty;
        public decimal ChangePercent { get; set; }
        public string Direction { get; set; } = "up";
    }

    public class WeeklyShipmentBarDto
    {
        public string Label { get; set; } = string.Empty;
        public int Delivered { get; set; }
        public int Pending { get; set; }
        public int Failed { get; set; }
    }

    public class CourierPerformanceDto
    {
        public string Name { get; set; } = string.Empty;
        public int Shipments { get; set; }
        public decimal SuccessRate { get; set; }
    }

    public class DashboardRecentOrderDto
    {
        public string Id { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
    }

    public class SuperAdminRecentUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Joined { get; set; }
    }

    public class SuperAdminRecentOrderDto
    {
        public string Id { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Courier { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
