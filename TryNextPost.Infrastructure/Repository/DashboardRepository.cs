using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;

        public DashboardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CountOrdersAsync(long? sellerId, DateTime? from, DateTime? to, bool pendingOnly = false)
        {
            var query = _context.Orders.AsQueryable().Where(o => o.IsActive == true);

            if (sellerId.HasValue)
                query = query.Where(o => o.SellerId == sellerId.Value);

            if (pendingOnly)
                query = query.Where(o => o.Status == OrderStatus.Pending);

            if (from.HasValue || to.HasValue)
                query = ApplyDateRange(query, from, to, o => o.CreatedAt);

            return await query.CountAsync();
        }

        public async Task<int> CountShipmentsAsync(
            long? sellerId,
            DateTime? from,
            DateTime? to,
            IEnumerable<int>? statusFilter = null)
        {
            var query = BaseShipmentQuery(sellerId);

            if (statusFilter != null)
            {
                var statuses = statusFilter.ToList();
                query = query.Where(s => statuses.Contains((int)s.Status));
            }

            if (from.HasValue || to.HasValue)
                query = ApplyDateRange(query, from, to, s => s.CreatedAt);

            return await query.CountAsync();
        }

        public async Task<decimal> SumOrderRevenueAsync(long? sellerId, DateTime? from, DateTime? to)
        {
            var query = _context.Orders.AsQueryable()
                .Where(o => o.IsActive == true && o.Status == OrderStatus.Delivered);

            if (sellerId.HasValue)
                query = query.Where(o => o.SellerId == sellerId.Value);

            if (from.HasValue || to.HasValue)
                query = ApplyDateRange(query, from, to, o => o.UpdatedAt ?? o.CreatedAt);

            return await query.SumAsync(o => (decimal?)o.FinalPayableAmount) ?? 0m;
        }

        public async Task<decimal> SumPlatformRevenueAsync(DateTime? from, DateTime? to)
        {
            var rechargeQuery = _context.WalletRecharges.AsQueryable()
                .Where(r => r.IsActive == true && r.Status == WalletRechargeStatus.Paid);

            if (from.HasValue || to.HasValue)
                rechargeQuery = ApplyDateRange(rechargeQuery, from, to, r => r.UpdatedAt ?? r.CreatedAt);

            var rechargeTotal = await rechargeQuery.SumAsync(r => (decimal?)r.Amount) ?? 0m;

            var marginQuery = _context.ShipmentCharges.AsQueryable().Where(c => c.IsActive == true);
            if (from.HasValue || to.HasValue)
                marginQuery = ApplyDateRange(marginQuery, from, to, c => c.CreatedAt);

            var marginTotal = await marginQuery.SumAsync(c => (decimal?)c.Margin) ?? 0m;

            return rechargeTotal + marginTotal;
        }

        public async Task<int> CountOpenNdrAsync(long? sellerId)
        {
            var query =
                from ndr in _context.NDRS
                join shipment in _context.Shipments on ndr.ShipmentId equals shipment.ShipmentId
                join order in _context.Orders on shipment.OrderId equals order.OrderId
                where ndr.IsActive == true
                    && (ndr.Status == NdrStatus.ActionRequired || ndr.Status == NdrStatus.ActionRequested)
                select new { ndr, order.SellerId };

            if (sellerId.HasValue)
                query = query.Where(x => x.SellerId == sellerId.Value);

            return await query.CountAsync();
        }

        public async Task<int> CountRtoAsync(long? sellerId, DateTime? from, DateTime? to)
        {
            var shipmentRto = BaseShipmentQuery(sellerId)
                .Where(s => s.Status == ShipmentStatus.RTO);

            if (from.HasValue || to.HasValue)
                shipmentRto = ApplyDateRange(shipmentRto, from, to, s => s.UpdatedAt ?? s.CreatedAt);

            var shipmentCount = await shipmentRto.CountAsync();

            var orderRto = _context.Orders.AsQueryable()
                .Where(o => o.IsActive == true && o.Status == OrderStatus.RTO);

            if (sellerId.HasValue)
                orderRto = orderRto.Where(o => o.SellerId == sellerId.Value);

            if (from.HasValue || to.HasValue)
                orderRto = ApplyDateRange(orderRto, from, to, o => o.UpdatedAt ?? o.CreatedAt);

            var orderCount = await orderRto.CountAsync();

            return shipmentCount + orderCount;
        }

        public async Task<decimal?> GetWalletBalanceAsync(long sellerId)
        {
            return await _context.Wallets
                .Where(w => w.SellerId == sellerId && w.IsActive == true)
                .Select(w => (decimal?)w.Balance)
                .FirstOrDefaultAsync();
        }

        public async Task<List<(DateTime Day, int Delivered, int Pending, int Failed)>> GetDailyShipmentBreakdownAsync(
            long sellerId, DateTime from, DateTime to)
        {
            var shipments = await BaseShipmentQuery(sellerId)
                .Where(s => s.CreatedAt >= from && s.CreatedAt < to)
                .Select(s => new { s.CreatedAt, s.Status })
                .ToListAsync();

            return shipments
                .GroupBy(s => s.CreatedAt!.Value.Date)
                .Select(g => (
                    Day: g.Key,
                    Delivered: g.Count(x => x.Status == ShipmentStatus.Delivered),
                    Pending: g.Count(x => IsPendingShipmentStatus(x.Status)),
                    Failed: g.Count(x => IsFailedShipmentStatus(x.Status))))
                .OrderBy(x => x.Day)
                .ToList();
        }

        public async Task<List<(string CourierName, int Total, int Delivered)>> GetCourierPerformanceAsync(
            long sellerId, DateTime from, DateTime to)
        {
            var rows = await BaseShipmentQuery(sellerId)
                .Where(s => s.CreatedAt >= from && s.CreatedAt < to)
                .Select(s => new
                {
                    CourierName = s.Courier != null ? s.Courier.CourierName : "Unknown",
                    s.Status
                })
                .ToListAsync();

            return rows
                .GroupBy(r => r.CourierName)
                .Select(g => (
                    CourierName: g.Key,
                    Total: g.Count(),
                    Delivered: g.Count(x => x.Status == ShipmentStatus.Delivered)))
                .OrderByDescending(x => x.Total)
                .ToList();
        }

        public async Task<List<Order>> GetRecentOrdersAsync(long? sellerId, int take)
        {
            var query = _context.Orders.AsQueryable()
                .Where(o => o.IsActive == true);

            if (sellerId.HasValue)
                query = query.Where(o => o.SellerId == sellerId.Value);

            return await query
                .OrderByDescending(o => o.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> CountSellersAsync(DateTime? from, DateTime? to)
        {
            var query = _context.Sellers.AsQueryable().Where(s => s.IsActive == true);

            if (from.HasValue || to.HasValue)
            {
                var userIds = _context.Users.AsQueryable();
                if (from.HasValue)
                    userIds = userIds.Where(u => u.CreatedAt >= from.Value);
                if (to.HasValue)
                    userIds = userIds.Where(u => u.CreatedAt < to.Value);

                var ids = await userIds.Select(u => u.Id).ToListAsync();
                query = query.Where(s => ids.Contains(s.UserId));
            }

            return await query.CountAsync();
        }

        public async Task<int> CountActiveCouriersAsync()
        {
            return await _context.Couriers.CountAsync(c => c.IsActive == true);
        }

        public async Task<List<(Seller Seller, string FullName, string Email, DateTime Joined)>> GetRecentSellersAsync(int take)
        {
            var rows = await (
                from seller in _context.Sellers
                join user in _context.Users on seller.UserId equals user.Id
                orderby user.CreatedAt descending
                select new
                {
                    Seller = seller,
                    user.FullName,
                    user.Email,
                    user.CreatedAt
                })
                .Take(take)
                .ToListAsync();

            return rows
                .Select(r => (r.Seller, r.FullName, r.Email ?? string.Empty, r.CreatedAt))
                .ToList();
        }

        public async Task<string?> GetCompanyNameAsync(long companyId)
        {
            return await _context.Companies
                .Where(c => c.CompanyId == companyId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();
        }

        public async Task<int> CountActiveOrdersPlatformAsync(DateTime? from, DateTime? to)
        {
            var query = _context.Orders.AsQueryable()
                .Where(o => o.IsActive == true
                    && o.Status != OrderStatus.Delivered
                    && o.Status != OrderStatus.Cancelled
                    && o.Status != OrderStatus.RTO);

            if (from.HasValue || to.HasValue)
                query = ApplyDateRange(query, from, to, o => o.CreatedAt);

            return await query.CountAsync();
        }

        public async Task<List<SuperAdminRecentOrderRow>> GetSuperAdminRecentOrdersAsync(int take)
        {
            var orders = await _context.Orders.AsQueryable()
                .Where(o => o.IsActive == true)
                .OrderByDescending(o => o.CreatedAt)
                .Take(take)
                .Select(o => new
                {
                    o.OrderRef,
                    o.FinalPayableAmount,
                    o.OrderId,
                    o.Status,
                    o.SellerId,
                    SellerCompany = o.Seller != null && o.Seller.Company != null ? o.Seller.Company.Name : null,
                    SellerUserName = o.Seller != null ? o.Seller.UserId : null
                })
                .ToListAsync();

            var orderIds = orders.Select(o => o.OrderId).ToList();
            var shipments = await _context.Shipments
                .Where(s => orderIds.Contains(s.OrderId) && s.IsActive == true)
                .Select(s => new
                {
                    s.OrderId,
                    CourierName = s.Courier != null ? s.Courier.CourierName : "—",
                    s.Status,
                    s.CreatedAt
                })
                .ToListAsync();

            var userIds = orders
                .Where(o => !string.IsNullOrEmpty(o.SellerUserName))
                .Select(o => o.SellerUserName!)
                .Distinct()
                .ToList();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName })
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            return orders.Select(o =>
            {
                var shipment = shipments
                    .Where(s => s.OrderId == o.OrderId)
                    .OrderByDescending(s => s.CreatedAt)
                    .FirstOrDefault();

                var sellerName = o.SellerCompany
                    ?? (o.SellerUserName != null && users.TryGetValue(o.SellerUserName, out var name)
                        ? name
                        : string.Format(SystemMessage.SellerNameFallback, o.SellerId));

                return new SuperAdminRecentOrderRow
                {
                    OrderRef = o.OrderRef,
                    Amount = o.FinalPayableAmount,
                    SellerDisplayName = sellerName,
                    CourierName = shipment?.CourierName ?? "—",
                    Status = shipment != null
                        ? MapShipmentStatusForDashboard(shipment.Status)
                        : MapOrderStatusForDashboard(o.Status)
                };
            }).ToList();
        }

        private static string MapOrderStatusForDashboard(OrderStatus status) => status switch
        {
            OrderStatus.Pending => "pending",
            OrderStatus.Confirmed or OrderStatus.Packed => "processing",
            OrderStatus.Shipped => "shipped",
            OrderStatus.Delivered => "delivered",
            OrderStatus.Cancelled => "cancelled",
            OrderStatus.RTO => "failed",
            _ => "pending"
        };

        private static string MapShipmentStatusForDashboard(ShipmentStatus status) => status switch
        {
            ShipmentStatus.Delivered => "delivered",
            ShipmentStatus.InTransit or ShipmentStatus.OutForDelivery or ShipmentStatus.PickedUp or ShipmentStatus.Picked => "in-transit",
            ShipmentStatus.Booked or ShipmentStatus.PendingPickup or ShipmentStatus.Created => "pending",
            ShipmentStatus.RTO or ShipmentStatus.Exception or ShipmentStatus.BookingFailed => "failed",
            ShipmentStatus.Cancelled => "cancelled",
            _ => "processing"
        };

        private IQueryable<Shipment> BaseShipmentQuery(long? sellerId)
        {
            var query = _context.Shipments.AsQueryable()
                .Where(s => s.IsActive == true);

            if (sellerId.HasValue)
                query = query.Where(s => s.Order != null && s.Order.SellerId == sellerId.Value);

            return query;
        }

        private static IQueryable<T> ApplyDateRange<T>(
            IQueryable<T> query,
            DateTime? from,
            DateTime? to,
            System.Linq.Expressions.Expression<Func<T, DateTime?>> dateSelector)
        {
            if (from.HasValue)
                query = query.Where(BuildCompareExpression(dateSelector, from.Value, isGreaterOrEqual: true));
            if (to.HasValue)
                query = query.Where(BuildCompareExpression(dateSelector, to.Value, isGreaterOrEqual: false));
            return query;
        }

        private static System.Linq.Expressions.Expression<Func<T, bool>> BuildCompareExpression<T>(
            System.Linq.Expressions.Expression<Func<T, DateTime?>> dateSelector,
            DateTime value,
            bool isGreaterOrEqual)
        {
            var parameter = dateSelector.Parameters[0];
            var body = dateSelector.Body;
            var constant = System.Linq.Expressions.Expression.Constant(value, typeof(DateTime?));
            System.Linq.Expressions.Expression comparison = isGreaterOrEqual
                ? System.Linq.Expressions.Expression.GreaterThanOrEqual(body, constant)
                : System.Linq.Expressions.Expression.LessThan(body, constant);
            return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(comparison, parameter);
        }

        private static bool IsPendingShipmentStatus(ShipmentStatus status) =>
            status is ShipmentStatus.Booked
                or ShipmentStatus.PendingPickup
                or ShipmentStatus.Created
                or ShipmentStatus.PickedUp
                or ShipmentStatus.Picked
                or ShipmentStatus.InTransit
                or ShipmentStatus.OutForDelivery
                or ShipmentStatus.ReachedDestination;

        private static bool IsFailedShipmentStatus(ShipmentStatus status) =>
            status is ShipmentStatus.RTO
                or ShipmentStatus.Exception
                or ShipmentStatus.Cancelled
                or ShipmentStatus.BookingFailed;
    }
}
