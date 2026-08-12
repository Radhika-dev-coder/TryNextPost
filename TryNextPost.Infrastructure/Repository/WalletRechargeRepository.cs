using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class WalletRechargeRepository : IWalletRechargeRepository
    {
        private readonly AppDbContext _context;

        public WalletRechargeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(WalletRecharge recharge)
        {
            await _context.WalletRecharges.AddAsync(recharge);
        }

        public async Task<WalletRecharge?> GetByIdAsync(long walletRechargeId)
        {
            return await _context.WalletRecharges
                .FirstOrDefaultAsync(r => r.WalletRechargeId == walletRechargeId && r.IsActive == true);
        }

        public async Task<WalletRecharge?> GetByGatewayOrderIdAsync(string gatewayOrderId)
        {
            return await _context.WalletRecharges
                .FirstOrDefaultAsync(r => r.GatewayOrderId == gatewayOrderId && r.IsActive == true);
        }

        public Task UpdateAsync(WalletRecharge recharge)
        {
            _context.WalletRecharges.Update(recharge);
            return Task.CompletedTask;
        }

        public async Task<decimal> SumPaidForSellerPeriodAsync(
            long sellerId,
            DateTime periodFrom,
            DateTime periodTo)
        {
            var end = periodTo.Date.AddDays(1);
            return await (
                from r in _context.WalletRecharges.AsNoTracking()
                join w in _context.Wallets.AsNoTracking() on r.WalletId equals w.WalletId
                where w.SellerId == sellerId
                      && r.IsActive == true
                      && r.Status == Domain.Enums.WalletRechargeStatus.Paid
                      && r.CreatedAt >= periodFrom
                      && r.CreatedAt < end
                select (decimal?)r.Amount
            ).SumAsync() ?? 0m;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
