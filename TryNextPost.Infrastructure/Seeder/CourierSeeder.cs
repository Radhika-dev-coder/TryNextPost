using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Seeder
{
    /// <summary>
    /// Seeds Courier master rows with codes matching ICourierAdapter implementations.
    /// </summary>
    public static class CourierSeeder
    {
        private static readonly (string Code, string Name, bool Cod, bool Prepaid, decimal? MaxWeightKg)[] SeedRows =
        [
            (CourierCodes.Delhivery, "Delhivery", true, true, 50m),
            (CourierCodes.BlueDart, "BlueDart", true, true, 50m),
            (CourierCodes.Xpressbees, "Xpressbees", true, true, 50m),
            (CourierCodes.Dtdc, "DTDC", true, true, 50m),
            (CourierCodes.Ekart, "Ekart", true, true, 50m),
            (CourierCodes.IndiaPost, "India Post", true, true, 30m),
            (CourierCodes.Shadowfax, "Shadowfax", true, true, 50m)
        ];

        public static async Task SeedAsync(AppDbContext db, ILogger? logger = null)
        {
            foreach (var row in SeedRows)
            {
                var exists = await db.Couriers
                    .AnyAsync(c => c.CourierCode == row.Code);

                if (exists)
                    continue;

                db.Couriers.Add(new Courier
                {
                    CourierCode = row.Code,
                    CourierName = row.Name,
                    SupportsCOD = row.Cod,
                    SupportsPrepaid = row.Prepaid,
                    MaxWeightLimit = row.MaxWeightKg,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "CourierSeeder"
                });

                logger?.LogInformation("Seeded courier master: {Code} ({Name})", row.Code, row.Name);
            }

            await db.SaveChangesAsync();
        }
    }
}
