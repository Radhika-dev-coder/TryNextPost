using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Seeder
{
    /// <summary>
    /// Seeds sample weight discrepancies (from booked shipments) and product weight freeze rows for testing.
    /// </summary>
    public static class WeightSeeder
    {
        public static async Task SeedAsync(AppDbContext db, ILogger? logger = null)
        {
            await SeedDiscrepanciesAsync(db, logger);
            await SeedFreezesAsync(db, logger);
        }

        private static async Task SeedDiscrepanciesAsync(AppDbContext db, ILogger? logger)
        {
            if (await db.WeightDiscrepancies.AnyAsync())
                return;

            var shipments = await db.Shipments
                .AsNoTracking()
                .Include(s => s.Order)!
                    .ThenInclude(o => o!.OrderItems)
                .Include(s => s.Courier)
                .Where(s => s.IsActive == true && s.AwbNumber != null && s.Order != null)
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .ToListAsync();

            if (shipments.Count == 0)
            {
                logger?.LogInformation("Weight discrepancy seed skipped — no shipments with AWB.");
                return;
            }

            var statuses = new[]
            {
                WeightDiscrepancyStatus.ActionRequired,
                WeightDiscrepancyStatus.Accepted,
                WeightDiscrepancyStatus.OpenDispute,
                WeightDiscrepancyStatus.ClosedDispute
            };

            var idx = 0;
            foreach (var shipment in shipments)
            {
                var entered = shipment.Weight > 0 ? shipment.Weight : (shipment.Order?.WeightGrams ?? 500);
                var applied = entered + 500 + (idx * 250);
                var charges = CalculateWeightCharges(entered, applied);
                var status = statuses[idx % statuses.Length];
                var product = shipment.Order?.OrderItems?.FirstOrDefault()?.ProductName ?? "Sample Product";

                db.WeightDiscrepancies.Add(new WeightDiscrepancy
                {
                    SellerId = shipment.Order!.SellerId,
                    ShipmentId = shipment.ShipmentId,
                    OrderId = shipment.OrderId,
                    AwbNumber = shipment.AwbNumber,
                    CourierId = shipment.CourierId,
                    CourierName = shipment.Courier?.CourierName,
                    ProductName = product,
                    EnteredWeightGrams = entered,
                    AppliedWeightGrams = applied,
                    WeightCharges = charges,
                    WeightAppliedDate = DateTime.UtcNow.AddDays(-idx),
                    Status = status,
                    DisputeRemarks = status is WeightDiscrepancyStatus.OpenDispute or WeightDiscrepancyStatus.ClosedDispute
                        ? "Sample dispute — courier weight audit"
                        : null,
                    AcceptedAt = status == WeightDiscrepancyStatus.Accepted ? DateTime.UtcNow.AddDays(-idx) : null,
                    DisputedAt = status is WeightDiscrepancyStatus.OpenDispute or WeightDiscrepancyStatus.ClosedDispute
                        ? DateTime.UtcNow.AddDays(-idx)
                        : null,
                    ClosedAt = status == WeightDiscrepancyStatus.ClosedDispute ? DateTime.UtcNow : null,
                    ClosedRemarks = status == WeightDiscrepancyStatus.ClosedDispute ? "Resolved in seller favour (sample)" : null,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-idx),
                    CreatedBy = "WeightSeeder"
                });

                idx++;
            }

            await db.SaveChangesAsync();
            logger?.LogInformation("Seeded {Count} weight discrepancy sample rows.", shipments.Count);
        }

        private static async Task SeedFreezesAsync(AppDbContext db, ILogger? logger)
        {
            if (await db.ProductWeightFreezes.AnyAsync())
                return;

            var seller = await db.Sellers.AsNoTracking().OrderBy(s => s.SellerId).FirstOrDefaultAsync();
            if (seller == null)
            {
                logger?.LogInformation("Weight freeze seed skipped — no sellers.");
                return;
            }

            var samples = new[]
            {
                new { Pid = "PID-1001", Name = "Cotton T-Shirt", Sku = "TS-001", L = 25m, B = 20m, H = 2m, W = 180m, Auto = true, Status = WeightFreezeStatus.Requested },
                new { Pid = "PID-1002", Name = "Running Shoes", Sku = "SH-042", L = 35m, B = 25m, H = 12m, W = 850m, Auto = false, Status = WeightFreezeStatus.Accepted },
                new { Pid = "PID-1003", Name = "Wireless Earbuds", Sku = "EB-007", L = 12m, B = 8m, H = 4m, W = 120m, Auto = true, Status = WeightFreezeStatus.Rejected }
            };

            foreach (var s in samples)
            {
                db.ProductWeightFreezes.Add(new ProductWeightFreeze
                {
                    SellerId = seller.SellerId,
                    ProductId = s.Pid,
                    ProductName = s.Name,
                    Sku = s.Sku,
                    LengthCm = s.L,
                    BreadthCm = s.B,
                    HeightCm = s.H,
                    WeightGrams = s.W,
                    AutoApply = s.Auto,
                    Status = s.Status,
                    ActionRemarks = s.Status == WeightFreezeStatus.Rejected ? "Dimensions mismatch on verification" : null,
                    ActionedAt = s.Status != WeightFreezeStatus.Requested ? DateTime.UtcNow : null,
                    ActionedBy = s.Status != WeightFreezeStatus.Requested ? "WeightSeeder" : null,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "WeightSeeder"
                });
            }

            await db.SaveChangesAsync();
            logger?.LogInformation("Seeded {Count} product weight freeze sample rows.", samples.Length);
        }

        private static decimal CalculateWeightCharges(decimal enteredGrams, decimal appliedGrams)
        {
            const decimal chargePerSlab = 25m;
            const decimal slabGrams = 500m;
            var diff = appliedGrams - enteredGrams;
            if (diff <= 0) return 0;
            return Math.Ceiling(diff / slabGrams) * chargePerSlab;
        }
    }
}
