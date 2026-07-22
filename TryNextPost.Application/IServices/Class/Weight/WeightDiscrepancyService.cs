using System.Globalization;
using System.Text;
using TryNextPost.Application.DTO.Weight;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.IServices.Interface.IWeight;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class.Weight
{
    public class WeightDiscrepancyService : IWeightDiscrepancyService
    {
        /// <summary>MVP flat formula: ₹25 per 500g slab above entered weight.</summary>
        public const decimal ChargePerSlab = 25m;
        public const decimal SlabGrams = 500m;

        private readonly IWeightDiscrepancyRepository _repository;
        private readonly ISellerContextService _sellerContextService;

        public WeightDiscrepancyService(
            IWeightDiscrepancyRepository repository,
            ISellerContextService sellerContextService)
        {
            _repository = repository;
            _sellerContextService = sellerContextService;
        }

        public async Task<WeightDiscrepancyListResponse> GetListAsync(
            string userId,
            bool isSuperAdmin,
            WeightDiscrepancyFilterRequest filter)
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
                filter.FromDate,
                filter.ToDate,
                filter.AwbNumbers,
                filter.ProductName,
                filter.CourierId);

            var totalCount = await _repository.GetFilteredCountAsync(
                sellerId,
                statusFilter,
                filter.FromDate,
                filter.ToDate,
                filter.AwbNumbers,
                filter.ProductName,
                filter.CourierId);

            var tabCounts = await BuildTabCountsAsync(sellerId);

            return new WeightDiscrepancyListResponse
            {
                Items = items.Select(Map).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TabCounts = tabCounts
            };
        }

        public async Task<WeightDiscrepancyListItemResponse> AcceptAsync(
            string userId,
            bool isSuperAdmin,
            long id)
        {
            var entity = await GetOwnedOrAdminAsync(userId, isSuperAdmin, id);

            if (entity.Status != WeightDiscrepancyStatus.ActionRequired)
                throw new InvalidOperationException(SystemMessage.WeightDiscrepancyActionNotAllowed);

            entity.Status = WeightDiscrepancyStatus.Accepted;
            entity.AcceptedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userId;

            // Wallet debit is phase-2; accept only updates status for MVP.
            await _repository.UpdateAsync(entity);
            await _repository.SaveChangesAsync();

            return Map(entity);
        }

        public async Task<WeightDiscrepancyListItemResponse> DisputeAsync(
            string userId,
            bool isSuperAdmin,
            long id,
            WeightDiscrepancyDisputeRequest request)
        {
            var entity = await GetOwnedOrAdminAsync(userId, isSuperAdmin, id);

            if (entity.Status != WeightDiscrepancyStatus.ActionRequired)
                throw new InvalidOperationException(SystemMessage.WeightDiscrepancyActionNotAllowed);

            entity.Status = WeightDiscrepancyStatus.OpenDispute;
            entity.DisputeRemarks = request.Remarks?.Trim();
            entity.DisputedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userId;

            await _repository.UpdateAsync(entity);
            await _repository.SaveChangesAsync();

            return Map(entity);
        }

        public async Task<WeightDiscrepancyListItemResponse> CloseDisputeAsync(
            string userId,
            long id,
            string? remarks)
        {
            var entity = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException(SystemMessage.WeightDiscrepancyNotFound);

            if (entity.Status != WeightDiscrepancyStatus.OpenDispute)
                throw new InvalidOperationException(SystemMessage.WeightDiscrepancyCloseNotAllowed);

            entity.Status = WeightDiscrepancyStatus.ClosedDispute;
            entity.ClosedRemarks = remarks?.Trim();
            entity.ClosedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userId;

            await _repository.UpdateAsync(entity);
            await _repository.SaveChangesAsync();

            return Map(entity);
        }

        public async Task<(byte[] Content, string FileName)> ExportCsvAsync(
            string userId,
            bool isSuperAdmin,
            WeightDiscrepancyFilterRequest filter)
        {
            var sellerId = await ResolveScopeSellerIdAsync(userId, isSuperAdmin);
            var statusFilter = ParseStatusTab(filter.StatusTab);

            var items = await _repository.GetFilteredAsync(
                sellerId,
                statusFilter,
                1,
                10000,
                filter.FromDate,
                filter.ToDate,
                filter.AwbNumbers,
                filter.ProductName,
                filter.CourierId);

            var sb = new StringBuilder();
            sb.AppendLine("WeightAppliedDate,AWBNumber,OrderId,OrderRef,EnteredWeightGrams,AppliedWeightGrams,WeightCharges,Product,Courier,Status");

            foreach (var item in items)
            {
                sb.Append(Csv(item.WeightAppliedDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))).Append(',');
                sb.Append(Csv(item.AwbNumber)).Append(',');
                sb.Append(Csv(item.OrderId?.ToString())).Append(',');
                sb.Append(Csv(item.Order?.OrderRef)).Append(',');
                sb.Append(Csv(item.EnteredWeightGrams.ToString(CultureInfo.InvariantCulture))).Append(',');
                sb.Append(Csv(item.AppliedWeightGrams.ToString(CultureInfo.InvariantCulture))).Append(',');
                sb.Append(Csv(item.WeightCharges.ToString(CultureInfo.InvariantCulture))).Append(',');
                sb.Append(Csv(item.ProductName)).Append(',');
                sb.Append(Csv(item.CourierName)).Append(',');
                sb.AppendLine(Csv(ToDisplayStatus(item.Status)));
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"weight-discrepancy-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return (bytes, fileName);
        }

        public static decimal CalculateWeightCharges(decimal enteredGrams, decimal appliedGrams)
        {
            var diff = appliedGrams - enteredGrams;
            if (diff <= 0)
                return 0;

            var slabs = Math.Ceiling(diff / SlabGrams);
            return slabs * ChargePerSlab;
        }

        private async Task<WeightDiscrepancy> GetOwnedOrAdminAsync(string userId, bool isSuperAdmin, long id)
        {
            var entity = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException(SystemMessage.WeightDiscrepancyNotFound);

            if (isSuperAdmin)
                return entity;

            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.ShipmentsView);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);
            if (entity.SellerId != seller.SellerId)
                throw new UnauthorizedAccessException(SystemMessage.Unauthorized);

            return entity;
        }

        private async Task<long?> ResolveScopeSellerIdAsync(string userId, bool isSuperAdmin)
        {
            if (isSuperAdmin)
                return null;

            await _sellerContextService.EnsurePermissionAsync(userId, EmployeePermissionCode.ShipmentsView);
            var seller = await _sellerContextService.ResolveSellerAsync(userId);
            return seller.SellerId;
        }

        private async Task<WeightDiscrepancyTabCounts> BuildTabCountsAsync(long? sellerId)
        {
            return new WeightDiscrepancyTabCounts
            {
                All = await _repository.GetCountByStatusAsync(sellerId, null),
                ActionRequired = await _repository.GetCountByStatusAsync(sellerId, WeightDiscrepancyStatus.ActionRequired),
                Accepted = await _repository.GetCountByStatusAsync(sellerId, WeightDiscrepancyStatus.Accepted),
                OpenDisputes = await _repository.GetCountByStatusAsync(sellerId, WeightDiscrepancyStatus.OpenDispute),
                ClosedDisputes = await _repository.GetCountByStatusAsync(sellerId, WeightDiscrepancyStatus.ClosedDispute)
            };
        }

        private static WeightDiscrepancyStatus? ParseStatusTab(string? statusTab)
        {
            if (string.IsNullOrWhiteSpace(statusTab) || statusTab.Equals("all", StringComparison.OrdinalIgnoreCase))
                return null;

            var key = statusTab.Trim().ToLowerInvariant()
                .Replace(" ", "", StringComparison.Ordinal)
                .Replace("_", "", StringComparison.Ordinal)
                .Replace("-", "", StringComparison.Ordinal);

            return key switch
            {
                "actionrequired" => WeightDiscrepancyStatus.ActionRequired,
                "accepted" => WeightDiscrepancyStatus.Accepted,
                "opendisputes" or "opendispute" => WeightDiscrepancyStatus.OpenDispute,
                "closeddisputes" or "closeddispute" => WeightDiscrepancyStatus.ClosedDispute,
                _ => throw new InvalidOperationException(SystemMessage.InvalidWeightDiscrepancyStatusTab)
            };
        }

        private static WeightDiscrepancyListItemResponse Map(WeightDiscrepancy entity)
        {
            return new WeightDiscrepancyListItemResponse
            {
                WeightDiscrepancyId = entity.WeightDiscrepancyId,
                WeightAppliedDate = entity.WeightAppliedDate,
                AwbNumber = entity.AwbNumber,
                OrderId = entity.OrderId,
                OrderRef = entity.Order?.OrderRef ?? string.Empty,
                EnteredWeightGrams = entity.EnteredWeightGrams,
                AppliedWeightGrams = entity.AppliedWeightGrams,
                WeightCharges = entity.WeightCharges,
                ProductName = entity.ProductName ?? string.Empty,
                CourierName = entity.CourierName,
                CourierId = entity.CourierId,
                Status = (int)entity.Status,
                StatusName = ToDisplayStatus(entity.Status),
                DisputeRemarks = entity.DisputeRemarks
            };
        }

        private static string ToDisplayStatus(WeightDiscrepancyStatus status) => status switch
        {
            WeightDiscrepancyStatus.ActionRequired => "Action Required",
            WeightDiscrepancyStatus.Accepted => "Accepted",
            WeightDiscrepancyStatus.OpenDispute => "Open Disputes",
            WeightDiscrepancyStatus.ClosedDispute => "Closed Disputes",
            _ => status.ToString()
        };

        private static string Csv(string? value)
        {
            var v = value ?? string.Empty;
            if (v.Contains(',') || v.Contains('"') || v.Contains('\n'))
                return $"\"{v.Replace("\"", "\"\"")}\"";
            return v;
        }
    }
}
