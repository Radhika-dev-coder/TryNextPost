using TryNextPost.Application.DTO.Weight;

namespace TryNextPost.Application.IServices.Interface.IWeight
{
    public interface IWeightDiscrepancyService
    {
        Task<WeightDiscrepancyListResponse> GetListAsync(string userId, bool isSuperAdmin, WeightDiscrepancyFilterRequest filter);
        Task<WeightDiscrepancyListItemResponse> AcceptAsync(string userId, bool isSuperAdmin, long id);
        Task<WeightDiscrepancyListItemResponse> DisputeAsync(string userId, bool isSuperAdmin, long id, WeightDiscrepancyDisputeRequest request);
        Task<WeightDiscrepancyListItemResponse> CloseDisputeAsync(string userId, long id, string? remarks);
        Task<(byte[] Content, string FileName)> ExportCsvAsync(string userId, bool isSuperAdmin, WeightDiscrepancyFilterRequest filter);
    }
}
