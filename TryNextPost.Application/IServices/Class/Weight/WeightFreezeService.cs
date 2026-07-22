using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using TryNextPost.Application.DTO.Weight;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.IServices.Interface.IWeight;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class.Weight
{
    public class WeightFreezeService : IWeightFreezeService
    {
        private readonly IProductWeightFreezeRepository _repository;
        private readonly ISellerContextService _sellerContextService;

        public WeightFreezeService(
            IProductWeightFreezeRepository repository,
            ISellerContextService sellerContextService)
        {
            _repository = repository;
            _sellerContextService = sellerContextService;
        }

        public async Task<WeightFreezeListResponse> GetListAsync(
            string userId,
            bool isSuperAdmin,
            WeightFreezeFilterRequest filter)
        {
            var sellerId = await ResolveScopeSellerIdAsync(userId, isSuperAdmin);
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 50 : Math.Min(filter.PageSize, 200);
            var statusFilter = ParseStatusTab(filter.StatusTab);

            var items = await _repository.GetFilteredAsync(
                sellerId,
                statusFilter,
                page,
                pageSize,
                filter.ProductSearch,
                filter.ProductId);

            var totalCount = await _repository.GetFilteredCountAsync(
                sellerId,
                statusFilter,
                filter.ProductSearch,
                filter.ProductId);

            var tabCounts = new WeightFreezeTabCounts
            {
                All = await _repository.GetCountByStatusAsync(sellerId, null),
                Requested = await _repository.GetCountByStatusAsync(sellerId, WeightFreezeStatus.Requested),
                Accepted = await _repository.GetCountByStatusAsync(sellerId, WeightFreezeStatus.Accepted),
                Rejected = await _repository.GetCountByStatusAsync(sellerId, WeightFreezeStatus.Rejected)
            };

            return new WeightFreezeListResponse
            {
                Items = items.Select(Map).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TabCounts = tabCounts
            };
        }

        public async Task<WeightFreezeListItemResponse> CreateAsync(string userId, CreateWeightFreezeRequest request)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.ShipmentsView);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);

            var entity = new ProductWeightFreeze
            {
                SellerId = seller.SellerId,
                ProductId = request.ProductId.Trim(),
                ProductName = request.ProductName.Trim(),
                Sku = request.Sku?.Trim(),
                LengthCm = request.LengthCm,
                BreadthCm = request.BreadthCm,
                HeightCm = request.HeightCm,
                WeightGrams = request.WeightGrams,
                AutoApply = request.AutoApply,
                Status = WeightFreezeStatus.Requested,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            return Map(entity);
        }

        public async Task<WeightFreezeListItemResponse> TakeActionAsync(
            string adminUserId,
            long id,
            WeightFreezeActionRequest request)
        {
            var entity = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException(SystemMessage.WeightFreezeNotFound);

            if (entity.Status != WeightFreezeStatus.Requested)
                throw new InvalidOperationException(SystemMessage.WeightFreezeActionNotAllowed);

            var actionKey = request.Action.Trim().ToLowerInvariant();
            if (actionKey is "accept" or "accepted")
            {
                entity.Status = WeightFreezeStatus.Accepted;
            }
            else if (actionKey is "reject" or "rejected")
            {
                entity.Status = WeightFreezeStatus.Rejected;
            }
            else
            {
                throw new InvalidOperationException(SystemMessage.WeightFreezeActionInvalid);
            }

            entity.ActionRemarks = request.Remarks?.Trim();
            entity.ActionedAt = DateTime.UtcNow;
            entity.ActionedBy = adminUserId;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = adminUserId;

            await _repository.UpdateAsync(entity);
            await _repository.SaveChangesAsync();

            return Map(entity);
        }

        public async Task<WeightFreezeImportResult> ImportCsvAsync(string userId, IFormFile file)
        {
            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.ShipmentsView);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);

            if (file == null || file.Length == 0)
                throw new InvalidOperationException(SystemMessage.WeightFreezeImportEmpty);

            var result = new WeightFreezeImportResult();
            var toAdd = new List<ProductWeightFreeze>();

            using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
            var headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(headerLine))
                throw new InvalidOperationException(SystemMessage.WeightFreezeImportEmpty);

            var headers = SplitCsvLine(headerLine)
                .Select(h => h.Trim().ToLowerInvariant())
                .ToList();

            int IndexOf(params string[] names)
            {
                foreach (var name in names)
                {
                    var idx = headers.IndexOf(name);
                    if (idx >= 0) return idx;
                }
                return -1;
            }

            var pidIdx = IndexOf("productid", "pid");
            var nameIdx = IndexOf("productname", "product", "productdetails");
            var skuIdx = IndexOf("sku");
            var lIdx = IndexOf("lengthcm", "length", "l");
            var bIdx = IndexOf("breadthcm", "breadth", "width", "b");
            var hIdx = IndexOf("heightcm", "height", "h");
            var wIdx = IndexOf("weightgrams", "weight", "wt");
            var autoIdx = IndexOf("autoapply", "auto_apply");

            if (pidIdx < 0 || nameIdx < 0 || wIdx < 0)
                throw new InvalidOperationException(
                    "CSV must include ProductId (or PID), ProductName, and WeightGrams columns.");

            var lineNo = 1;
            while (!reader.EndOfStream)
            {
                lineNo++;
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var cols = SplitCsvLine(line);
                try
                {
                    string Col(int idx) => idx >= 0 && idx < cols.Count ? cols[idx].Trim() : string.Empty;

                    var pid = Col(pidIdx);
                    var name = Col(nameIdx);
                    if (string.IsNullOrWhiteSpace(pid) || string.IsNullOrWhiteSpace(name))
                    {
                        result.SkippedCount++;
                        result.Errors.Add($"Line {lineNo}: ProductId and ProductName are required.");
                        continue;
                    }

                    if (!decimal.TryParse(Col(wIdx), NumberStyles.Any, CultureInfo.InvariantCulture, out var weight)
                        || weight <= 0)
                    {
                        result.SkippedCount++;
                        result.Errors.Add($"Line {lineNo}: Invalid WeightGrams.");
                        continue;
                    }

                    decimal ParseDim(int idx)
                    {
                        if (idx < 0) return 0;
                        return decimal.TryParse(Col(idx), NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
                            ? d
                            : 0;
                    }

                    var autoApply = true;
                    if (autoIdx >= 0)
                    {
                        var raw = Col(autoIdx).ToLowerInvariant();
                        autoApply = raw is "1" or "true" or "yes" or "y";
                    }

                    toAdd.Add(new ProductWeightFreeze
                    {
                        SellerId = seller.SellerId,
                        ProductId = pid,
                        ProductName = name,
                        Sku = skuIdx >= 0 ? NullIfEmpty(Col(skuIdx)) : null,
                        LengthCm = ParseDim(lIdx),
                        BreadthCm = ParseDim(bIdx),
                        HeightCm = ParseDim(hIdx),
                        WeightGrams = weight,
                        AutoApply = autoApply,
                        Status = WeightFreezeStatus.Requested,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId
                    });
                }
                catch (Exception ex)
                {
                    result.SkippedCount++;
                    result.Errors.Add($"Line {lineNo}: {ex.Message}");
                }
            }

            if (toAdd.Count > 0)
            {
                await _repository.AddRangeAsync(toAdd);
                await _repository.SaveChangesAsync();
            }

            result.ImportedCount = toAdd.Count;
            return result;
        }

        public async Task<(byte[] Content, string FileName)> ExportCsvAsync(
            string userId,
            bool isSuperAdmin,
            WeightFreezeFilterRequest filter)
        {
            var sellerId = await ResolveScopeSellerIdAsync(userId, isSuperAdmin);
            var statusFilter = ParseStatusTab(filter.StatusTab);

            var items = await _repository.GetFilteredAsync(
                sellerId,
                statusFilter,
                1,
                10000,
                filter.ProductSearch,
                filter.ProductId);

            var sb = new StringBuilder();
            sb.AppendLine("PID,ProductName,SKU,LengthCm,BreadthCm,HeightCm,WeightGrams,AutoApply,Status");

            foreach (var item in items)
            {
                sb.Append(Csv(item.ProductId)).Append(',');
                sb.Append(Csv(item.ProductName)).Append(',');
                sb.Append(Csv(item.Sku)).Append(',');
                sb.Append(Csv(item.LengthCm.ToString(CultureInfo.InvariantCulture))).Append(',');
                sb.Append(Csv(item.BreadthCm.ToString(CultureInfo.InvariantCulture))).Append(',');
                sb.Append(Csv(item.HeightCm.ToString(CultureInfo.InvariantCulture))).Append(',');
                sb.Append(Csv(item.WeightGrams.ToString(CultureInfo.InvariantCulture))).Append(',');
                sb.Append(Csv(item.AutoApply ? "Yes" : "No")).Append(',');
                sb.AppendLine(Csv(ToDisplayStatus(item.Status)));
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"weight-freeze-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return (bytes, fileName);
        }

        private async Task<long?> ResolveScopeSellerIdAsync(string userId, bool isSuperAdmin)
        {
            if (isSuperAdmin)
                return null;

            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.ShipmentsView);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);
            return seller.SellerId;
        }

        private static WeightFreezeStatus? ParseStatusTab(string? statusTab)
        {
            if (string.IsNullOrWhiteSpace(statusTab) || statusTab.Equals("all", StringComparison.OrdinalIgnoreCase))
                return null;

            var key = statusTab.Trim().ToLowerInvariant()
                .Replace(" ", "", StringComparison.Ordinal)
                .Replace("_", "", StringComparison.Ordinal)
                .Replace("-", "", StringComparison.Ordinal);

            return key switch
            {
                "requested" => WeightFreezeStatus.Requested,
                "accepted" => WeightFreezeStatus.Accepted,
                "rejected" => WeightFreezeStatus.Rejected,
                _ => throw new InvalidOperationException(SystemMessage.InvalidWeightFreezeStatusTab)
            };
        }

        private static WeightFreezeListItemResponse Map(ProductWeightFreeze entity)
        {
            return new WeightFreezeListItemResponse
            {
                ProductWeightFreezeId = entity.ProductWeightFreezeId,
                ProductId = entity.ProductId,
                ProductName = entity.ProductName,
                Sku = entity.Sku,
                LengthCm = entity.LengthCm,
                BreadthCm = entity.BreadthCm,
                HeightCm = entity.HeightCm,
                Dimensions = $"{entity.LengthCm} x {entity.BreadthCm} x {entity.HeightCm}",
                WeightGrams = entity.WeightGrams,
                AutoApply = entity.AutoApply,
                Status = (int)entity.Status,
                StatusName = ToDisplayStatus(entity.Status),
                ActionRemarks = entity.ActionRemarks,
                CreatedAt = entity.CreatedAt
            };
        }

        private static string ToDisplayStatus(WeightFreezeStatus status) => status switch
        {
            WeightFreezeStatus.Requested => "Requested",
            WeightFreezeStatus.Accepted => "Accepted",
            WeightFreezeStatus.Rejected => "Rejected",
            _ => status.ToString()
        };

        private static string? NullIfEmpty(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : value;

        private static string Csv(string? value)
        {
            var v = value ?? string.Empty;
            if (v.Contains(',') || v.Contains('"') || v.Contains('\n'))
                return $"\"{v.Replace("\"", "\"\"")}\"";
            return v;
        }

        private static List<string> SplitCsvLine(string line)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }

            result.Add(sb.ToString());
            return result;
        }
    }
}
