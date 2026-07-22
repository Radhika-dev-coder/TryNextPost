using Microsoft.AspNetCore.Http;
using TryNextPost.Application.DTO.Weight;

namespace TryNextPost.Application.IServices.Interface.IWeight
{
    public interface IWeightFreezeService
    {
        Task<WeightFreezeListResponse> GetListAsync(string userId, bool isSuperAdmin, WeightFreezeFilterRequest filter);
        Task<WeightFreezeListItemResponse> CreateAsync(string userId, CreateWeightFreezeRequest request);
        Task<WeightFreezeListItemResponse> TakeActionAsync(string adminUserId, long id, WeightFreezeActionRequest request);
        Task<WeightFreezeImportResult> ImportCsvAsync(string userId, IFormFile file);
        Task<(byte[] Content, string FileName)> ExportCsvAsync(string userId, bool isSuperAdmin, WeightFreezeFilterRequest filter);
    }
}
