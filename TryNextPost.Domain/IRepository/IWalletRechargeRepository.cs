using TryNextPost.Domain.Entities;

namespace TryNextPost.Domain.IRepository
{
    public interface IWalletRechargeRepository
    {
        Task AddAsync(WalletRecharge recharge);
        Task<WalletRecharge?> GetByIdAsync(long walletRechargeId);
        Task<WalletRecharge?> GetByGatewayOrderIdAsync(string gatewayOrderId);
        Task UpdateAsync(WalletRecharge recharge);
        Task SaveChangesAsync();
    }
}
