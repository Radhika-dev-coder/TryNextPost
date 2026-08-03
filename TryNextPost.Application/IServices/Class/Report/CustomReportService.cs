using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Report;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.IServices.Interface.IReport;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Common.Report;
using TryNextPost.Domain.Constants;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Entities.Report;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository.Report;
using OrderEntity = TryNextPost.Domain.Entities.Order;
using ShipmentEntity = TryNextPost.Domain.Entities.Shipment;


namespace TryNextPost.Application.IServices.Class.Report
{
    public class CustomReportService : ICustomReportService
    {
        private readonly IExportHistoryRepository _exportHistoryRepository;
        private readonly ISellerContextService _sellerContextService;
        private readonly IWebHostEnvironment _env;

        // Field key => (CSV header, value getter)
        private static readonly IReadOnlyDictionary<string, (string Header, Func<OrderEntity, ShipmentEntity?, string> Value)> FieldMap =
            new Dictionary<string, (string, Func<OrderEntity, ShipmentEntity?, string>)>(StringComparer.OrdinalIgnoreCase)
            {
                [CustomReportFieldKeys.OrderNumber] = ("Order Number", (o, _) => o.OrderRef),
                [CustomReportFieldKeys.OrderDate] = ("Order Date", (o, _) => o.OrderDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                [CustomReportFieldKeys.OrderStatus] = ("Order Status", (o, _) => o.Status.ToString()),
                [CustomReportFieldKeys.PaymentMode] = ("Payment Mode", (o, _) => o.PaymentMode.ToString()),
                [CustomReportFieldKeys.OrderType] = ("Order Type", (o, _) => o.OrderType.ToString()),
                [CustomReportFieldKeys.Channel] = ("Channel", (o, _) => o.Channel ?? ""),
                [CustomReportFieldKeys.CustomerName] = ("Customer Name", (o, _) => o.CustomerName),
                [CustomReportFieldKeys.CustomerMobile] = ("Customer Mobile", (o, _) => o.CustomerMobile),
                [CustomReportFieldKeys.ShippingCity] = ("Shipping City", (o, _) => o.ShippingCity),
                [CustomReportFieldKeys.ShippingState] = ("Shipping State", (o, _) => o.ShippingState),
                [CustomReportFieldKeys.ShippingPincode] = ("Shipping Pincode", (o, _) => o.ShippingPincode),
                [CustomReportFieldKeys.ProductNames] = ("Products", (o, _) => string.Join(" | ", o.OrderItems.Select(i => $"{i.ProductName} x{i.Qty}"))),
                [CustomReportFieldKeys.TotalAmount] = ("Total Amount", (o, _) => o.TotalAmount.ToString(CultureInfo.InvariantCulture)),
                [CustomReportFieldKeys.FinalPayableAmount] = ("Final Payable", (o, _) => o.FinalPayableAmount.ToString(CultureInfo.InvariantCulture)),
                [CustomReportFieldKeys.CodCharges] = ("COD Charges", (o, _) => o.CodCharges.ToString(CultureInfo.InvariantCulture)),
                [CustomReportFieldKeys.ShippingCharges] = ("Shipping Charges", (o, _) => o.ShippingCharges.ToString(CultureInfo.InvariantCulture)),
                [CustomReportFieldKeys.OrderWeightGrams] = ("Order Weight (g)", (o, _) => o.WeightGrams.ToString(CultureInfo.InvariantCulture)),
                [CustomReportFieldKeys.Awb] = ("AWB", (_, s) => s?.AwbNumber ?? ""),
                [CustomReportFieldKeys.ShipmentStatus] = ("Shipment Status", (_, s) => s?.Status.ToString() ?? ""),
                [CustomReportFieldKeys.CourierName] = ("Courier", (_, s) => s?.Courier?.CourierName ?? ""),
                [CustomReportFieldKeys.ServiceCode] = ("Service Code", (_, s) => s?.ServiceCode ?? ""),
                [CustomReportFieldKeys.ChargedAmount] = ("Charged Amount", (_, s) => s?.ChargedAmount.ToString(CultureInfo.InvariantCulture) ?? ""),
                [CustomReportFieldKeys.ShipmentWeight] = ("Shipment Weight", (_, s) => s?.Weight.ToString(CultureInfo.InvariantCulture) ?? ""),
                [CustomReportFieldKeys.WarehouseName] = ("Warehouse Name", (o, _) => o.PickupAddress?.WarehouseName ?? o.PickupAddress?.Name ?? ""),
                [CustomReportFieldKeys.WarehouseCity] = ("Warehouse City", (o, _) => o.PickupAddress?.City ?? ""),
                [CustomReportFieldKeys.WarehouseState] = ("Warehouse State", (o, _) => o.PickupAddress?.State ?? ""),
                [CustomReportFieldKeys.WarehousePincode] = ("Warehouse Pincode", (o, _) => o.PickupAddress?.Pincode ?? ""),
                [CustomReportFieldKeys.WarehouseMobile] = ("Warehouse Mobile", (o, _) => o.PickupAddress?.Mobile ?? "")
            };

        public CustomReportService(
        IExportHistoryRepository exportHistoryRepository,
        ISellerContextService sellerContextService,
        IWebHostEnvironment env)
        {
            _exportHistoryRepository = exportHistoryRepository;
            _sellerContextService = sellerContextService;
            _env = env;
        }
        public async Task<(byte[] Content, string FileName)> DownloadExportAsync(string userId, long exportHistoryId)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.OrdersView);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);
            var export = await _exportHistoryRepository.GetByIdAsync(exportHistoryId)
                ?? throw new KeyNotFoundException(SystemMessage.ExportHistoryNotFound);
            if (export.SellerId != seller.SellerId)
                throw new UnauthorizedAccessException(SystemMessage.Unauthorized);
            if (export.Status != ExportHistoryStatus.Completed)
                throw new InvalidOperationException(SystemMessage.ExportHistoryNotReady);
            if (string.IsNullOrWhiteSpace(export.FilePath))
                throw new InvalidOperationException(SystemMessage.ExportHistoryFileMissing);
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var fullPath = Path.Combine(webRoot, export.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
                throw new InvalidOperationException(SystemMessage.ExportHistoryFileMissing);
            var content = await File.ReadAllBytesAsync(fullPath);
            return (content, export.FileName);
        }

        public async Task<(byte[] Content, string FileName, string ContentType, long ExportHistoryId)> GenerateCustomReportAsync(string userId, CustomReportRequest request)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.OrdersView);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);
            if (request.FromDate == default || request.ToDate == default)
                throw new InvalidOperationException(SystemMessage.CustomReportDateRequired);
            if (request.ToDate.Date < request.FromDate.Date)
                throw new InvalidOperationException(SystemMessage.CustomReportDateRangeInvalid);
            var fields = (request.Fields ?? new List<string>())
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Select(f => f.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (fields.Count == 0)
                throw new InvalidOperationException(SystemMessage.CustomReportFieldsRequired);
            if (fields.Any(f => !CustomReportFieldKeys.IsValid(f)))
                throw new InvalidOperationException(SystemMessage.CustomReportFieldsInvalid);
            var columns = fields
                .Select(f => FieldMap.First(kv => kv.Key.Equals(f, StringComparison.OrdinalIgnoreCase)).Value)
                .ToList();
            var orders = await _exportHistoryRepository.GetOrdersForCustomReportAsync(
                seller.SellerId, request.FromDate, request.ToDate);
            var shipmentMap = await _exportHistoryRepository.GetLatestShipmentsByOrderIdsAsync(
                orders.Select(o => o.OrderId));
            byte[] content;
            string fileName;
            string contentType;

            if (string.Equals(request.Format, "xlsx", StringComparison.OrdinalIgnoreCase))
            {
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Report");
                for (int col = 0; col < columns.Count; col++)
                {
                    var headerCell = ws.Cell(1, col + 1);
                    headerCell.Value = columns[col].Header;
                    headerCell.Style.Font.Bold = true;
                }
                for (int rowIdx = 0; rowIdx < orders.Count; rowIdx++)
                {
                    var order = orders[rowIdx];
                    shipmentMap.TryGetValue(order.OrderId, out var shipment);
                    for (int col = 0; col < columns.Count; col++)
                        ws.Cell(rowIdx + 2, col + 1).Value = columns[col].Value(order, shipment);
                }
                ws.Columns().AdjustToContents();
                using var ms = new MemoryStream();
                workbook.SaveAs(ms);
                content = ms.ToArray();
                fileName = $"custom-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.xlsx";
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }
            else
            {
                var sb = new StringBuilder();
                sb.AppendLine(string.Join(",", columns.Select(c => Csv(c.Header))));
                foreach (var order in orders)
                {
                    shipmentMap.TryGetValue(order.OrderId, out var shipment);
                    var row = columns.Select(c => Csv(c.Value(order, shipment)));
                    sb.AppendLine(string.Join(",", row));
                }
                content = Encoding.UTF8.GetBytes(sb.ToString());
                fileName = $"custom-report-{seller.SellerId}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
                contentType = "text/csv";
            }
            var relativePath = await SaveFileAsync(seller.SellerId, fileName, content);
            var history = new ExportHistory
            {
                SellerId = seller.SellerId,
                ReportType = "CustomReport",
                FromDate = request.FromDate.Date,
                ToDate = request.ToDate.Date,
                SelectedFields = string.Join(",", fields),
                Status = ExportHistoryStatus.Completed,
                FileName = fileName,
                FilePath = relativePath,
                RowCount = orders.Count,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            await _exportHistoryRepository.AddAsync(history);
            await _exportHistoryRepository.SaveChangesAsync();
            return (content, fileName, contentType, history.ExportHistoryId);
        }

        public async Task<ExportHistoryListResponse> GetExportHistoryAsync(string userId, int page, int pageSize)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.OrdersView);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 100);
            var (items, total) = await _exportHistoryRepository.GetBySellerAsync(seller.SellerId, page, pageSize);
            return new ExportHistoryListResponse
            {
                Items = items.Select(e => new ExportHistoryListItemResponse
                {
                    ExportHistoryId = e.ExportHistoryId,
                    ReportType = e.ReportType,
                    FromDate = e.FromDate,
                    ToDate = e.ToDate,
                    SelectedFields = e.SelectedFields,
                    Status = e.Status.ToString(),
                    FileName = e.FileName,
                    RowCount = e.RowCount,
                    CreatedAt = e.CreatedAt
                }).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        private async Task<string> SaveFileAsync(long sellerId, string fileName, byte[] bytes)
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var folder = Path.Combine(webRoot, "uploads", "reports", sellerId.ToString(CultureInfo.InvariantCulture));
            Directory.CreateDirectory(folder);
            var fullPath = Path.Combine(folder, fileName);
            await File.WriteAllBytesAsync(fullPath, bytes);
            return $"/uploads/reports/{sellerId}/{fileName}";
        }
        private static string Csv(string? value)
        {
            var v = value ?? string.Empty;
            if (v.Contains(',') || v.Contains('"') || v.Contains('\n'))
                return $"\"{v.Replace("\"", "\"\"")}\"";
            return v;
        }

        public async Task<ShipmentSummaryResponse> GetShipmentSummaryAsync(string userId, ShipmentSummaryRequest request)
        {

            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.OrdersView);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);

            if (request.FromDate == default || request.ToDate == default)
                throw new InvalidOperationException(SystemMessage.CustomReportDateRequired);
            if (request.ToDate.Date < request.FromDate.Date)
                throw new InvalidOperationException(SystemMessage.CustomReportDateRangeInvalid);

            var counts = await _exportHistoryRepository.GetShipmentStatusCountsAsync(
                seller.SellerId, request.FromDate, request.ToDate);

            return new ShipmentSummaryResponse
            {
                Booked = counts.GetValueOrDefault(ShipmentStatus.Booked),
                ShipmentPicked = counts.GetValueOrDefault(ShipmentStatus.PickedUp),   // Picked aur PickedUp same value (2) hai
                Delivered = counts.GetValueOrDefault(ShipmentStatus.Delivered),
                RtoInitiated = counts.GetValueOrDefault(ShipmentStatus.RTOInitiated),
                RtoDelivered = counts.GetValueOrDefault(ShipmentStatus.RTODelivered),
                RtoAcknowledged = counts.GetValueOrDefault(ShipmentStatus.RTOAcknowledged)
            };
        }

        public async Task<object> ExportReportAsync(string userId, ReportRequest request)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.OrdersView);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);

            var filter = new ReportFilter
            {
                ReportType = request.ReportType,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Courier = request.Courier,
                State = request.State,
                ProductName = request.ProductName,
                Channel = request.Channel,
                Zone = request.Zone
            };

            var data = await _exportHistoryRepository.GetReportDataAsync(seller.SellerId, filter);

            var fileBytes = GenerateCsv(data);

            var fileName = $"Report_{DateTime.Now:yyyyMMddHHmmss}.csv";

            var filePath = await SaveFileAsync(seller.SellerId, fileName, fileBytes);

            var history = new ExportHistory
            {
                SellerId = seller.SellerId,
                ReportType = request.ReportType.ToString(),
                FromDate = request.FromDate?.Date ?? DateTime.UtcNow.Date,
                ToDate = request.ToDate?.Date ?? DateTime.UtcNow.Date,
                Status = ExportHistoryStatus.Completed,
                FileName = fileName,
                FilePath = filePath,
                RowCount = data.Count,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            await _exportHistoryRepository.AddAsync(history);

            await _exportHistoryRepository.SaveChangesAsync();

            return new { FileName = fileName, Url = filePath };
        }

        private byte[] GenerateCsv<T>(List<T> data)
        {
            var props = typeof(T).GetProperties();
            var sb = new StringBuilder();

            sb.AppendLine(string.Join(",", props.Select(p => p.Name)));

            foreach (var item in data)
            {
                var values = props.Select(p => Csv(p.GetValue(item)?.ToString()));
                sb.AppendLine(string.Join(",", values));
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<List<DailySummaryResponse>> GetDailySummaryDataAsync(string userId, DailySummaryRequest request)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.OrdersView);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);

            if (request.FromDate == default || request.ToDate == default)
                throw new InvalidOperationException(SystemMessage.CustomReportDateRequired);

            if (request.ToDate.Date < request.FromDate.Date)
                throw new InvalidOperationException(SystemMessage.CustomReportDateRangeInvalid);

            var data = await _exportHistoryRepository.GetDailySummaryDataAsync(
                seller.SellerId,request.FromDate,request.ToDate);

            return data.Select(x => new DailySummaryResponse
            {
                Date = x.Date,

                ShipmentPicked = x.ShipmentPicked,
                InTransit = x.InTransit,
                Exception = x.Exception,
                Delivered = x.Delivered,

                RTOInTransit = x.RTOInTransit,
                RTODelivered = x.RTODelivered
            }).ToList();
        }
    }
}
