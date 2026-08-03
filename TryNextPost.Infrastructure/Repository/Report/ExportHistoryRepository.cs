using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.IRepository.Report;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Entities.Report;
using TryNextPost.Infrastructure.AppDbContexts;
using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Common.Report;

namespace TryNextPost.Infrastructure.Repository.Report
{
    public class ExportHistoryRepository : IExportHistoryRepository
    {
        private readonly AppDbContext _appDbContext;

        public ExportHistoryRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddAsync(ExportHistory entity)
        {
            await _appDbContext.ExportHistories.AddAsync(entity);
        }

        public async Task<ExportHistory?> GetByIdAsync(long exportHistoryId)
        {
            return await _appDbContext.ExportHistories.FirstOrDefaultAsync(e => e.ExportHistoryId == exportHistoryId
             && e.IsActive == true);
        }

        //public async Task<(List<ExportHistory> Items, int TotalCount)> GetBySellerAsync(long sellerId, int page, int pageSize)
        //{
        //    var query = _appDbContext.ExportHistories.AsNoTracking().Where(e => e.SellerId == sellerId
        //    && e.IsActive == true);

        //    var total = await query.CountAsync();
        //    var items = await query.OrderByDescending(e => e.CreatedAt)
        //        .ThenByDescending(e => e.ExportHistoryId)
        //        .Skip((page -1) * pageSize)
        //        .Take(pageSize).ToListAsync();
        //    return(items, total);
        //}

        public async Task<Dictionary<long, Shipment>> GetLatestShipmentsByOrderIdsAsync(IEnumerable<long> orderIds)
        {
            var ids = orderIds.Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<long, Shipment>();

            var shipments = await _appDbContext.Shipments
                .AsNoTracking()
                .Include(s => s.Courier)
                .Where(s => ids.Contains(s.OrderId) && s.IsActive == true).ToListAsync();

            return shipments.GroupBy(s => s.OrderId)
                .ToDictionary(g => g.Key,
                g => g.OrderByDescending(s => s.ShipmentId).First());
        }

        public async Task<List<Order>> GetOrdersForCustomReportAsync(long sellerId, DateTime fromDate, DateTime toDate)
        {
            var endExclusice = toDate.Date.AddDays(1);

            return await _appDbContext.Orders.AsNoTracking()
                .Include(o => o.OrderItems)
                .Include(o => o.PickupAddress)
                .Where(o => o.SellerId == sellerId
                && o.IsActive == true
                && o.OrderDate >= fromDate.Date
                && o.OrderDate < endExclusice)
                .OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        public async Task<List<object>> GetReportDataAsync(long sellerId, ReportFilter filter)
        {
            switch (filter.ReportType)
            {
                case ReportType.CustomReport:
                    return await GetCustomReportData(sellerId, filter);

                case ReportType.ShipmentSummary:
                    var counts = await GetShipmentStatusCountsAsync(sellerId, filter.FromDate ?? DateTime.MinValue, filter.ToDate ?? DateTime.MaxValue);

                    var result = new List<object>
                    {
                        new { Label = "Shipment Picked", Count = counts.GetValueOrDefault(ShipmentStatus.PickedUp, 0) },
                        new { Label = "In Transit", Count = counts.GetValueOrDefault(ShipmentStatus.InTransit, 0) },
                        new { Label = "Exception", Count = counts.GetValueOrDefault(ShipmentStatus.Exception, 0) },
                        new { Label = "Delivered", Count = counts.GetValueOrDefault(ShipmentStatus.Delivered, 0) },
                        new { Label = "RTO In Transit", Count = counts.GetValueOrDefault(ShipmentStatus.RTOInTransit, 0) },
                        new { Label = "RTO Delivered", Count = counts.GetValueOrDefault(ShipmentStatus.RTODelivered, 0) }
                    };
                    return result;

                case ReportType.TopNdrReasons:
                    return await GetTopNdrData(sellerId, filter);

                case ReportType.DailySummary:
                    return await GetDailySummaryData(sellerId, filter);

                default:
                    throw new Exception("Report not implemented yet");
            }
        }

        private async Task<List<object>> GetCustomReportData(long sellerId, ReportFilter filter)
        {
            var query = from o in _appDbContext.Orders
                        join s in _appDbContext.Shipments
                            on o.OrderId equals s.OrderId into shipmentGroup
                        from s in shipmentGroup.DefaultIfEmpty()
                        where o.SellerId == sellerId
                        select new
                        {
                            o.OrderId,o.OrderRef,o.OrderDate,o.CustomerName,o.CustomerMobile,
                            o.ShippingCity,o.ShippingState,o.TotalAmount,Courier = (int?)s.CourierId,
                            AwbNumber = s.AwbNumber,ShipmentStatus = (int?)s.Status
                        };
            if (filter.FromDate.HasValue)
                query = query.Where(x => x.OrderDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(x => x.OrderDate <= filter.ToDate.Value);

            if (!string.IsNullOrEmpty(filter.State))
                query = query.Where(x => x.ShippingState == filter.State);

            if (!string.IsNullOrEmpty(filter.Courier))
                query = query.Where(x => x.Courier.ToString() == filter.Courier);

            return await query.ToListAsync<object>();
        }

        private async Task<List<object>> GetDailySummaryData(long sellerId,ReportFilter filter)
        {
            var fromDate = filter.FromDate ?? DateTime.MinValue;
            var toDate = filter.ToDate ?? DateTime.MaxValue;
            var endExclusive = toDate.Date.AddDays(1);

            var data = await _appDbContext.Shipments
                .AsNoTracking()
                .Where(s => s.Order.SellerId == sellerId
                         && s.IsActive == true
                         && s.CreatedAt >= fromDate.Date
                         && s.CreatedAt < endExclusive)
                .GroupBy(s => s.CreatedAt.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,

                    ShipmentPicked = g.Count(x => x.Status == ShipmentStatus.PickedUp),
                    InTransit = g.Count(x => x.Status == ShipmentStatus.InTransit),
                    Exception = g.Count(x => x.Status == ShipmentStatus.Exception),
                    Delivered = g.Count(x => x.Status == ShipmentStatus.Delivered),

                    RTOInTransit = g.Count(x => x.Status == ShipmentStatus.RTOInTransit),
                    RTODelivered = g.Count(x => x.Status == ShipmentStatus.RTODelivered)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return data.Cast<object>().ToList();
        }


        private async Task<List<object>> GetTopNdrData(long sellerId, ReportFilter filter)
        {
            var query = from o in _appDbContext.Orders
                        join s in _appDbContext.Shipments
                            on o.OrderId equals s.OrderId
                        join n in _appDbContext.NDRS on s.ShipmentId equals n.ShipmentId
                        where o.SellerId == sellerId && s.Status == ShipmentStatus.NDR
                        select new
                        {
                            NdrReason = n.Reason
                        };

            var result = await query
                .GroupBy(x => x.NdrReason)
                .Select(g => new
                {
                    Reason = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            return result.Cast<object>().ToList();
        }

        public async Task<(List<ExportHistory> Items, int TotalCount)> GetBySellerAsync(
            long sellerId, int page, int pageSize)
        {
            var query = _appDbContext.ExportHistories
                .Where(x => x.SellerId == sellerId);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<Dictionary<ShipmentStatus, int>> GetShipmentStatusCountsAsync(long sellerId, DateTime fromDate, DateTime toDate)
        {
            var endExclusive = toDate.Date.AddDays(1);

            var counts = await _appDbContext.Shipments
                .AsNoTracking()
                .Where(s => s.Order.SellerId == sellerId
                         && s.IsActive == true
                         && s.CreatedAt >= fromDate.Date
                         && s.CreatedAt < endExclusive)
                .GroupBy(s => s.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return counts.ToDictionary(x => x.Status, x => x.Count);
        }

        public async Task SaveChangesAsync()
        {
            await _appDbContext.SaveChangesAsync();
        }

        public Task UpdateAsync(ExportHistory entity)
        {
            _appDbContext.ExportHistories.Update(entity);
            return Task.CompletedTask;
        }



        public async Task<List<DailySummaryData>> GetDailySummaryDataAsync(long sellerId, DateTime fromDate, DateTime toDate)
        {
            var data = await _appDbContext.Shipments
                 .Where(s => s.Order.SellerId == sellerId
                          && s.IsActive == true
                          && s.CreatedAt >= fromDate.Date
                          && s.CreatedAt < toDate.Date.AddDays(1))
                 .GroupBy(s => s.CreatedAt.Value.Date)
                 .Select(g => new DailySummaryData
                 {
                     Date = g.Key,
                     ShipmentPicked = g.Count(x => x.Status == ShipmentStatus.PickedUp),
                     Delivered = g.Count(x => x.Status == ShipmentStatus.Delivered),
                     InTransit = g.Count(x => x.Status == ShipmentStatus.InTransit),
                     Exception = g.Count(x => x.Status == ShipmentStatus.Exception),
                     RtoInTransit = g.Count(x => x.Status == ShipmentStatus.RTOInTransit),
                     RtoDelivered = g.Count(x => x.Status == ShipmentStatus.RTODelivered)
                 })
                 .ToListAsync();

            return Enumerable.Range(0, (toDate.Date - fromDate.Date).Days + 1)
                .Select(i =>
                {
                    var d = fromDate.Date.AddDays(i);
                    var x = data.FirstOrDefault(a => a.Date == d);

                    return x ?? new DailySummaryData { Date = d };
                })
                .OrderBy(x => x.Date)
                .ToList();
        }
    }
}
