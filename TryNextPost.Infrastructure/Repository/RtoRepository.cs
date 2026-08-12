using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class RtoRepository : IRtoRepository
    {
        private readonly AppDbContext _context;

        public RtoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RTO rto)
        {
            await _context.RTOS.AddAsync(rto);
        }

        public async Task<RTO?> GetOpenByShipmentIdAsync(long shipmentId)
        {
            return await _context.RTOS
                .Where(r =>
                    r.ShipmentId == shipmentId
                    && r.IsActive == true
                    && (r.Status == RtoStatus.Initiated || r.Status == RtoStatus.InTransit))
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
