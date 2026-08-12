using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Entities.Report;
using TryNextPost.Domain.Common.Report;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.IRepository.Report
{
    public interface IExportHistoryRepository
    {
        Task AddAsync(ExportHistory entity);
        Task UpdateAsync(ExportHistory entity);
        Task SaveChangesAsync();

        Task<ExportHistory?> GetByIdAsync(long exportHistoryId);
        Task<(List<ExportHistory> Items, int TotalCount)> GetBySellerAsync(
            long sellerId, int page, int pageSize);
        Task<List<Order>> GetOrdersForCustomReportAsync(
            long sellerId, DateTime fromDate, DateTime toDate);
        Task<Dictionary<long, Shipment>> GetLatestShipmentsByOrderIdsAsync(
            IEnumerable<long> orderIds);

        Task<Dictionary<ShipmentStatus, int>> GetShipmentStatusCountsAsync(long sellerId, DateTime fromDate, DateTime toDate);

        Task<List<object>> GetReportDataAsync(long sellerId, ReportFilter filter);
        Task<List<DailySummaryData>> GetDailySummaryDataAsync(long sellerId, DateTime fromDate, DateTime toDate);

        Task<List<StateWiseSummaryData>> GetStateWiseSummaryDataAsync(long sellerId, DateTime fromDate, DateTime toDate, string? courier, string? paymentMethod);

        Task<List<TopNdrReasonsData>> GetTopNdrReasonsDataAsync( long sellerId, DateTime fromDate, DateTime toDate);

        Task<List<ProductWiseSummaryData>> GetProductWiseSummaryDataAsync(long sellerId, DateTime fromDate, DateTime toDate, string? productName);

        Task<List<CourierWiseSummaryData>> GetCourierWiseSummaryDataAsync(long sellerId, DateTime fromDate, DateTime toDate, string? courier);

        Task<List<ChannelSummaryData>> GetChannelWiseSummary(long sellerId, DateTime fromDate, DateTime toDate, string? channel);

        Task<List<ZoneSummaryData>> GetZoneWiseSummary(long sellerId, DateTime fromDate, DateTime toDate, string? zone);
    }
}
