using TryNextPost.Application.DTO.Settlement;

namespace TryNextPost.Application.IServices.Interface.ISettlement
{
    public interface ICourierSettlementService
    {
        Task<CourierSettlementSummaryResponse> GetSummaryAsync(
            long courierId,
            DateTime periodFrom,
            DateTime periodTo);

        Task<CourierSettlementResponse> CreateSettlementBatchAsync(
            CreateCourierSettlementRequest request,
            string adminUserId);

        Task<CourierSettlementResponse> MarkAsPaidAsync(
            MarkCourierSettlementPaidRequest request,
            string adminUserId);

        Task<List<CourierSettlementResponse>> GetSettlementsAsync(
            long? courierId,
            DateTime? from,
            DateTime? to);
    }
}
