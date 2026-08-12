using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Application.DTO.Report;

namespace TryNextPost.Application.IServices.Interface.IReport
{
    public interface ICustomReportService
    {
        Task<(byte[] Content, string FileName, string ContentType, long ExportHistoryId)> GenerateCustomReportAsync(
        string userId, CustomReportRequest request);
        Task<ExportHistoryListResponse> GetExportHistoryAsync(string userId, int page, int pageSize);
        Task<(byte[] Content, string FileName)> DownloadExportAsync(string userId, long exportHistoryId);

        Task<ShipmentSummaryResponse> GetShipmentSummaryAsync(string userId, ShipmentSummaryRequest request);

        Task<object> ExportReportAsync(string userId, ReportRequest request);

        Task<List<DailySummaryResponse>> GetDailySummaryDataAsync(string userId, DailySummaryRequest request);

        Task<List<StateWiseSummaryResponse>> GetStateWiseSummaryAsync(string userId, StateWiseSummaryRequest request);

        Task<List<TopNdrReasonsResponse>> GetTopNdrReasonsAsync(string userId, TopNdrReasonsRequest request);

        Task<List<ProductWiseSummaryResponse>> GetProductWiseSummaryAsync(string userId, ProductWiseSummaryRequest request);

        Task<List<CourierWiseSummaryResponse>> GetCourierWiseSummaryAsync(string userId, CourierWiseSummaryRequest request);

        Task<List<ChannelSummaryResponse>> GetChannelWiseSummaryAsync(string userId, ChannelSummaryRequest request);
    }
}
