using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TryNextPost.Domain.Entities;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Seeder
{
    /// <summary>
    /// Seeds 5 zones (Z1–Z5) from pincode prefix and sample rate cards for all couriers.
    /// </summary>
    public static class RateCardSeeder
    {
        private static readonly (string Code, string Name)[] Zones =
        [
            ("Z1", "North"),
            ("Z2", "West"),
            ("Z3", "Central"),
            ("Z4", "East"),
            ("Z5", "South")
        ];

        public static async Task SeedAsync(AppDbContext db, ILogger? logger = null)
        {
            await SeedZonesAsync(db, logger);
            await SeedRateCardsAsync(db, logger);
        }

        private static async Task SeedZonesAsync(AppDbContext db, ILogger? logger)
        {
            var zoneIds = new Dictionary<string, int>();

            foreach (var (code, name) in Zones)
            {
                var existing = await db.Zones.FirstOrDefaultAsync(z => z.ZoneCode == code);
                if (existing == null)
                {
                    existing = new Zone
                    {
                        ZoneCode = code,
                        ZoneName = name,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "RateCardSeeder"
                    };
                    db.Zones.Add(existing);
                    await db.SaveChangesAsync();
                    logger?.LogInformation("Seeded zone {Code} ({Name})", code, name);
                }

                zoneIds[code] = existing.ZoneId;
            }

            var existingPrefixes = await db.PincodeZoneMappings
                .AsNoTracking()
                .Select(m => m.PincodePrefix)
                .ToHashSetAsync();

            for (var prefix = 0; prefix <= 99; prefix++)
            {
                var prefixStr = prefix.ToString("D2");
                if (existingPrefixes.Contains(prefixStr))
                    continue;

                var zoneCode = ResolveZoneCode(prefix);
                db.PincodeZoneMappings.Add(new PincodeZoneMapping
                {
                    PincodePrefix = prefixStr,
                    ZoneId = zoneIds[zoneCode],
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "RateCardSeeder"
                });
            }

            await db.SaveChangesAsync();
        }

        private static string ResolveZoneCode(int prefix)
        {
            // Simple MVP mapping by first digit of prefix.
            var firstDigit = prefix / 10;
            return firstDigit switch
            {
                0 or 1 => "Z1",
                2 or 3 => "Z2",
                4 or 5 => "Z3",
                6 or 7 => "Z4",
                _ => "Z5"
            };
        }

        private static async Task SeedRateCardsAsync(AppDbContext db, ILogger? logger)
        {
            var couriers = await db.Couriers
                .Where(c => c.IsActive == true)
                .ToListAsync();

            var zones = await db.Zones.Where(z => z.IsActive == true).ToListAsync();
            if (couriers.Count == 0 || zones.Count == 0)
                return;

            var slabs = new (decimal From, decimal To, decimal CourierCost, decimal SellerCharge)[]
            {
                (0, 500, 35m, 50m),
                (501, 1000, 45m, 65m),
                (1001, 3000, 65m, 90m),
                (3001, 5000, 85m, 115m),
                (5001, 10000, 110m, 145m),
                (10001, 50000, 150m, 195m)
            };

            var existingKeys = await db.CourierRateCards
                .AsNoTracking()
                .Select(r => new
                {
                    r.CourierId,
                    r.FromZoneId,
                    r.ToZoneId,
                    r.WeightFromGrams,
                    r.WeightToGrams,
                    r.ServiceCode
                })
                .ToListAsync();

            var existingSet = existingKeys
                .Select(r => (r.CourierId, r.FromZoneId, r.ToZoneId, r.WeightFromGrams, r.WeightToGrams, r.ServiceCode))
                .ToHashSet();

            var pending = new List<CourierRateCard>();

            foreach (var courier in couriers)
            {
                foreach (var fromZone in zones)
                {
                    foreach (var toZone in zones)
                    {
                        var isSameZone = fromZone.ZoneId == toZone.ZoneId;
                        var zoneMultiplier = isSameZone ? 1.0m : 1.15m;

                        foreach (var slab in slabs)
                        {
                            var key = (courier.CourierId, fromZone.ZoneId, toZone.ZoneId, slab.From, slab.To, "SURFACE");
                            if (existingSet.Contains(key))
                                continue;

                            pending.Add(new CourierRateCard
                            {
                                CourierId = courier.CourierId,
                                FromZoneId = fromZone.ZoneId,
                                ToZoneId = toZone.ZoneId,
                                WeightFromGrams = slab.From,
                                WeightToGrams = slab.To,
                                CourierCost = Math.Round(slab.CourierCost * zoneMultiplier, 2),
                                SellerCharge = Math.Round(slab.SellerCharge * zoneMultiplier, 2),
                                ServiceCode = "SURFACE",
                                EstimatedDays = isSameZone ? 3 : 4,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = "RateCardSeeder"
                            });
                            existingSet.Add(key);
                        }
                    }
                }

                logger?.LogInformation("Prepared rate cards for courier {Code}", courier.CourierCode);
            }

            if (pending.Count > 0)
            {
                db.CourierRateCards.AddRange(pending);
                await db.SaveChangesAsync();
                logger?.LogInformation("Seeded {Count} rate card rows", pending.Count);
            }
        }
    }
}
