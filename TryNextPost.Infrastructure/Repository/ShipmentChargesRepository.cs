using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class ShipmentChargesRepository : IShipmentChargesRepository
    {
        private readonly AppDbContext _context;

        public ShipmentChargesRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ShipmentCharges charges)
        {
            await _context.ShipmentCharges.AddAsync(charges);
        }

        public async Task<ShipmentCharges?> GetByShipmentIdAsync(long shipmentId)
        {
            return await _context.ShipmentCharges
                .FirstOrDefaultAsync(c => c.ShipmentId == shipmentId && c.IsActive == true);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
