using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Report;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Common.Report;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Entities.Report;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository.Report;
using TryNextPost.Infrastructure.AppDbContexts;

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

                case ReportType.StateWiseSummary:
                    return await GetStateWiseSummaryData(sellerId, filter);

                case ReportType.ProductWiseSummary:
                    return await GetProductWiseSummaryData(sellerId, filter);
                default:
                    throw new Exception("Report not implemented yet");
            }
        }

        private async Task<List<object>> GetProductWiseSummaryData(long sellerId, ReportFilter filter)
        {
            var fromDate = filter.FromDate ?? DateTime.MinValue;
            var toDate = filter.ToDate ?? DateTime.MaxValue;

            var data = await GetProductWiseSummaryDataAsync(
                sellerId, fromDate, toDate, filter.ProductName);

            return data.Select(x => (object)new
            {
                ProductName = x.ProductName,
                ProductSku = x.Sku,
                TotalOrderQuantity = x.TotalOrderQuantity,
                Booked = x.Booked,
                PendingPickup = x.PendingPickup,
                InTransit = x.InTransit,
                Delivered = x.Delivered,
                RTO = x.RTO
            }).ToList();
        }

        private async Task<List<object>> GetStateWiseSummaryData(long sellerId, ReportFilter filter)
        {
            var fromDate = filter.FromDate ?? DateTime.MinValue;
            var toDate = filter.ToDate ?? DateTime.MaxValue;
            var endExclusive = toDate.Date.AddDays(1);

            var query = _appDbContext.Orders
                .AsNoTracking()
                .Where(o => o.SellerId == sellerId
                         && o.IsActive == true
                         && o.OrderDate >= fromDate.Date
                         && o.OrderDate < endExclusive)
                .GroupJoin(
                    _appDbContext.Shipments.Where(s => s.IsActive == true),
                    o => o.OrderId,
                    s => s.OrderId,
                    (o, shipments) => new { o, shipments }
                )
                    .SelectMany(
            x => x.shipments.DefaultIfEmpty(),
            (x, s) => new { Order = x.o, Shipment = s }
           );

             if (!string.IsNullOrEmpty(filter.Courier))
             {
                 query = query.Where(x => x.Shipment != null &&
                                          x.Shipment.CourierId.ToString() == filter.Courier);
             }
          

             if (filter.PaymentMethod.HasValue)
             {
                 query = query.Where(x => x.Order.PaymentMode == filter.PaymentMethod.Value);
             }
          
             var data = await query
                 .GroupBy(x => x.Order.ShippingState)
                 .Select(g => new
                 {
                     State = g.Key,
          
                     ShipmentPicked = g.Count(x =>
                         x.Shipment != null && (
                             (ShipmentStatus)x.Shipment.Status == ShipmentStatus.Created ||
                             (ShipmentStatus)x.Shipment.Status == ShipmentStatus.Booked ||
                             (ShipmentStatus)x.Shipment.Status == ShipmentStatus.PendingPickup ||
                             (ShipmentStatus)x.Shipment.Status == ShipmentStatus.PickupScheduled ||
                             (ShipmentStatus)x.Shipment.Status == ShipmentStatus.PickedUp
                         )
                     ),
          
                     InTransit = g.Count(x =>
                         x.Shipment != null && (
                             (ShipmentStatus)x.Shipment.Status == ShipmentStatus.InTransit ||
                             (ShipmentStatus)x.Shipment.Status == ShipmentStatus.ReachedDestination ||
                             (ShipmentStatus)x.Shipment.Status == ShipmentStatus.OutForDelivery
                         )
                     ),
          
                     Exception = g.Count(x =>
                         x.Shipment != null && (
                             (ShipmentStatus)x.Shipment.Status == ShipmentStatus.Exception ||
                             (ShipmentStatus)x.Shipment.Status == ShipmentStatus.Lost ||
                             (ShipmentStatus)x.Shipment.Status == ShipmentStatus.Damaged
                         )
                     ),
          
                     Delivered = g.Count(x =>
                         x.Shipment != null &&
                         (ShipmentStatus)x.Shipment.Status == ShipmentStatus.Delivered
                     ),
          
                     RTO = g.Count(x =>
                         x.Shipment != null && (
                             (ShipmentStatus)x.Shipment.Status == ShipmentStatus.RTOInitiated ||
                             (ShipmentStatus)x.Shipment.Status == ShipmentStatus.RTOInTransit ||
                             (ShipmentStatus)x.Shipment.Status == ShipmentStatus.RTODelivered ||
                             (ShipmentStatus)x.Shipment.Status == ShipmentStatus.RTOAcknowledged
                         )
                     )
                 })
                 .OrderBy(x => x.State)
                 .ToListAsync();
          
             return data.Cast<object>().ToList();
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

        public async Task<List<StateWiseSummaryData>> GetStateWiseSummaryDataAsync(long sellerId, DateTime fromDate, DateTime toDate, string? courier, string? paymentMethod)
        {
            var query = _appDbContext.Shipments.
                         Where(s => s.Order.SellerId == sellerId
                         && s.IsActive == true
                         && s.CreatedAt >= fromDate.Date
                         && s.CreatedAt < toDate.Date.AddDays(1));

            // Courier filter
            if (!string.IsNullOrEmpty(courier))
                query = query.Where(s => s.CourierId.ToString() == courier);

            // ✅ Payment Method filter
            if (!string.IsNullOrEmpty(paymentMethod))
            {
                if (paymentMethod == "COD")
                    query = query.Where(s => s.Order.PaymentMode == PaymentMode.COD);

                else if (paymentMethod == "Prepaid")
                    query = query.Where(s => s.Order.PaymentMode == PaymentMode.Prepaid);
            }
            var data = await query
           .GroupBy(s => s.Order.ShippingState)
            .Select(g => new StateWiseSummaryData
            {
                State = g.Key,
            
                ShipmentPicked = g.Count(x => x.Status == ShipmentStatus.PickedUp),
                InTransit = g.Count(x => x.Status == ShipmentStatus.InTransit),
                Exception = g.Count(x => x.Status == ShipmentStatus.Exception),
                Delivered = g.Count(x => x.Status == ShipmentStatus.Delivered),
            
                RTO = g.Count(x => x.Status == ShipmentStatus.RTOInTransit
                                || x.Status == ShipmentStatus.RTODelivered)
            })
            .ToListAsync();
            
                    return data;

        }

        public async Task<List<TopNdrReasonsData>> GetTopNdrReasonsDataAsync(long sellerId, DateTime fromDate, DateTime toDate)
        {
            var endExclusive = toDate.Date.AddDays(1);

            var query = from o in _appDbContext.Orders
                        join s in _appDbContext.Shipments on o.OrderId equals s.OrderId
                        join n in _appDbContext.NDRS on s.ShipmentId equals n.ShipmentId
                        where o.SellerId == sellerId
                                    && s.IsActive == true
                                    && s.Status == ShipmentStatus.NDR
                                    && s.CreatedAt >= fromDate.Date
                                    && s.CreatedAt < endExclusive
                        select new { NdrReason = n.Reason };

            return await query
                .GroupBy(x => x.NdrReason)
                .Select(g => new TopNdrReasonsData
                {
                    Reason = g.Key ?? string.Empty,
                    TotalCount = g.Count()
                })
                .OrderByDescending(x => x.TotalCount)
                .Take(10).ToListAsync();
        }

        public async Task<List<ProductWiseSummaryData>> GetProductWiseSummaryDataAsync(long sellerId, DateTime fromDate, DateTime toDate, string? productName)
        {
            var endExclusive = toDate.Date.AddDays(1);
            var itemsQuery = _appDbContext.OrderItems.AsNoTracking()
                .Where(oi => oi.Order!.SellerId == sellerId
                          && oi.Order.IsActive == true
                          && oi.Order.OrderDate >= fromDate.Date
                          && oi.Order.OrderDate < endExclusive);
            if (!string.IsNullOrWhiteSpace(productName))
            {
                var term = productName.Trim();
                itemsQuery = itemsQuery.Where(oi => oi.ProductName.Contains(term));
            }
            // 1) Qty by product
            var quantities = await itemsQuery
                .GroupBy(oi => new { oi.ProductName, Sku = oi.Sku ?? string.Empty })
                .Select(g => new
                {
                    g.Key.ProductName,
                    g.Key.Sku,
                    TotalOrderQuantity = g.Sum(x => x.Qty)
                })
                .ToListAsync();
            // 2) Status counts by product
            var statusRows = await (
                from oi in itemsQuery
                join s in _appDbContext.Shipments.AsNoTracking().Where(s => s.IsActive == true)
                    on oi.OrderId equals s.OrderId into sg
                from s in sg.DefaultIfEmpty()
                select new
                {
                    oi.ProductName,
                    Sku = oi.Sku ?? string.Empty,
                    Status = (ShipmentStatus?)(s != null ? s.Status : (ShipmentStatus?)null)
                }
            ).ToListAsync();
            var statusByProduct = statusRows
                .GroupBy(x => new { x.ProductName, x.Sku })
                .ToDictionary(
                    g => (g.Key.ProductName, g.Key.Sku),
                    g => new
                    {
                        Booked = g.Count(x =>
                            x.Status == ShipmentStatus.Created ||
                            x.Status == ShipmentStatus.Booked),
                        PendingPickup = g.Count(x =>
                            x.Status == ShipmentStatus.PendingPickup ||
                            x.Status == ShipmentStatus.PickupScheduled),
                        InTransit = g.Count(x =>
                            x.Status == ShipmentStatus.PickedUp ||
                            x.Status == ShipmentStatus.InTransit ||
                            x.Status == ShipmentStatus.ReachedDestination ||
                            x.Status == ShipmentStatus.OutForDelivery),
                        Delivered = g.Count(x => x.Status == ShipmentStatus.Delivered),
                        RTO = g.Count(x =>
                            x.Status == ShipmentStatus.RTOInitiated ||
                            x.Status == ShipmentStatus.RTOInTransit ||
                            x.Status == ShipmentStatus.RTODelivered ||
                            x.Status == ShipmentStatus.RTOAcknowledged)
                    });
            return quantities
                .Select(q =>
                {
                    statusByProduct.TryGetValue((q.ProductName, q.Sku), out var st);
                    return new ProductWiseSummaryData
                    {
                        ProductName = q.ProductName,
                        Sku = q.Sku,
                        TotalOrderQuantity = q.TotalOrderQuantity,
                        Booked = st?.Booked ?? 0,
                        PendingPickup = st?.PendingPickup ?? 0,
                        InTransit = st?.InTransit ?? 0,
                        Delivered = st?.Delivered ?? 0,
                        RTO = st?.RTO ?? 0
                    };
                })
                .OrderBy(x => x.ProductName)
                .ThenBy(x => x.Sku)
                .ToList();
        }

        public async Task<List<CourierWiseSummaryData>> GetCourierWiseSummaryDataAsync(long sellerId, DateTime fromDate, DateTime toDate, string? courier)
        {
            var endExclusive = toDate.Date.AddDays(1);

            var shipmentQuery = from o in _appDbContext.Orders
                                join s in _appDbContext.Shipments on o.OrderId equals s.OrderId
                                join c in _appDbContext.Couriers on s.CourierId equals c.CourierId
                                where o.SellerId == sellerId
                                 && s.IsActive == true
                         && s.CreatedAt >= fromDate.Date
                         && s.CreatedAt < endExclusive
                                select new
                                {
                                    s.CourierId,
                                    CourierName = c.CourierName,
                                    s.Status
                                };

            if (!string.IsNullOrWhiteSpace(courier))
            {
                var term = courier.Trim();
                shipmentQuery = shipmentQuery
                    .Where(x => x.CourierName != null && x.CourierName.Contains(term));
            }

            var data = await shipmentQuery
                .GroupBy(s => s.CourierName ?? "Unknown")
                .Select(g => new CourierWiseSummaryData
                {
                    CourierName = g.Key,
                    TotalShipped = g.Count(),

                    Booked = g.Count(x =>
                        x.Status == ShipmentStatus.Created ||
                        x.Status == ShipmentStatus.Booked),

                    PendingPickup = g.Count(x =>
                        x.Status == ShipmentStatus.PendingPickup ||
                        x.Status == ShipmentStatus.PickupScheduled),

                    InTransit = g.Count(x =>
                        x.Status == ShipmentStatus.PickedUp ||
                        x.Status == ShipmentStatus.InTransit ||
                        x.Status == ShipmentStatus.ReachedDestination ||
                        x.Status == ShipmentStatus.OutForDelivery),

                    Delivered = g.Count(x =>
                        x.Status == ShipmentStatus.Delivered),

                    RTO = g.Count(x =>
                        x.Status == ShipmentStatus.RTOInitiated ||
                        x.Status == ShipmentStatus.RTOInTransit ||
                        x.Status == ShipmentStatus.RTODelivered ||
                        x.Status == ShipmentStatus.RTOAcknowledged)
                })
                .OrderBy(x => x.CourierName)
                .ToListAsync();

            return data;
        }

        public async Task<List<ChannelSummaryData>> GetChannelWiseSummary(long sellerId, DateTime fromDate, DateTime toDate, string? channel)
        {

            var endExclusive = toDate.Date.AddDays(1);
            var query = from s in _appDbContext.Shipments
                        join o in _appDbContext.Orders
                            on s.OrderId equals o.OrderId
                        where o.SellerId == sellerId
                            && s.IsActive == true
                            && s.CreatedAt >= fromDate.Date
                            && s.CreatedAt < endExclusive
                        select new
                        {
                            ChannelName = o.Channel, 
                            s.Status
                        };

            if (!string.IsNullOrWhiteSpace(channel))
            {
                var term = channel.Trim();
                query = query.Where(x => x.ChannelName != null && x.ChannelName.Contains(term));
            }

            if (!string.IsNullOrEmpty(channel))
            {
                query = query.Where(x => x.ChannelName == channel);
            }

            var result = await query
                .GroupBy(x => x.ChannelName ?? "Unknown")
                .Select(g => new ChannelSummaryData
                {
                    ChannelName = g.Key,
                    TotalShipments = g.Count(),

                    Booked = g.Count(x =>
                        x.Status == ShipmentStatus.Created ||
                        x.Status == ShipmentStatus.Booked),

                    PendingPickup = g.Count(x =>
                        x.Status == ShipmentStatus.PendingPickup ||
                        x.Status == ShipmentStatus.PickupScheduled),

                    InTransit = g.Count(x =>
                        x.Status == ShipmentStatus.PickedUp ||
                        x.Status == ShipmentStatus.InTransit ||
                        x.Status == ShipmentStatus.ReachedDestination ||
                        x.Status == ShipmentStatus.OutForDelivery),

                    Delivered = g.Count(x =>
                        x.Status == ShipmentStatus.Delivered),

                    RTO = g.Count(x =>
                        x.Status == ShipmentStatus.RTOInitiated ||
                        x.Status == ShipmentStatus.RTOInTransit ||
                        x.Status == ShipmentStatus.RTODelivered ||
                        x.Status == ShipmentStatus.RTOAcknowledged)
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<ZoneSummaryData>> GetZoneWiseSummary(long sellerId, DateTime fromDate, DateTime toDate, string? zone)
        {
            //var endExclusive = toDate.Date.AddDays(1);

            //var query = from s in _appDbContext.Shipments
            //            join o in _appDbContext.Orders
            //                on s.OrderId equals o.OrderId
            //            where o.SellerId == sellerId
            //                && s.IsActive == true
            //                && s.CreatedAt >= fromDate.Date
            //                && s.CreatedAt < endExclusive
            //                && (string.IsNullOrEmpty(zone) || o.Zone == zone)
            //            select new
            //            {
            //                Zone = o.Zone,
            //                s.Status
            //            };

            //var data = await query
            //    .GroupBy(x => x.Zone)
            //    .Select(g => new ZoneSummaryResponse
            //    {
            //        ZoneName = g.Key,
            //        TotalShipment = g.Count(),
            //        Booked = g.Count(x => x.Status == "Booked"),
            //        PendingPickup = g.Count(x => x.Status == "PendingPickup"),
            //        InTransit = g.Count(x => x.Status == "InTransit"),
            //        Delivered = g.Count(x => x.Status == "Delivered"),
            //        RTO = g.Count(x => x.Status == "RTO")
            //    })
            //    .ToListAsync();

            //return data;

            throw new NotImplementedException();
        }
    }
}
