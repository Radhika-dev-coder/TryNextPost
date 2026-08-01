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
            if(ids.Count == 0)
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
                    return counts.Select(kv => new { Status = kv.Key.ToString(), Count = kv.Value }).Cast<object>().ToList();

                case ReportType.TopNdrReasons:
                    return await GetTopNdrData(sellerId, filter);

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
                            o.OrderId,o.OrderRef,o.OrderDate,o.CustomerName, o.CustomerMobile,
                            o.ShippingCity,o.ShippingState,o.TotalAmount,
                            Courier = (int?)s.CourierId,
                            AwbNumber = s.AwbNumber,
                            ShipmentStatus = (int?)s.Status
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
    }
}
