using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
