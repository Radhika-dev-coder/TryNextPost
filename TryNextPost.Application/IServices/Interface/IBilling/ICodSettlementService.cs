using TryNextPost.Application.DTO.Billing;

namespace TryNextPost.Application.IServices.Interface.IBilling
{
    public interface ICodSettlementService
    {
        Task<CodSettlementAdminListResponse> GetForAdminAsync(CodSettlementAdminFilterRequest filter);
        Task<CodRemittanceListItemResponse> MarkSettledAsync(string adminUserId, CodSettlementMarkSettledRequest request);
    }
}
