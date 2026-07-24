using Microsoft.AspNetCore.Http;
using TryNextPost.Application.DTO.Weight;

namespace TryNextPost.Application.IServices.Interface.IWeight
{
    public interface IWeightFreezeService
    {
        Task<WeightFreezeListResponse> GetListAsync(string userId, bool isSuperAdmin, WeightFreezeFilterRequest filter);
        Task<WeightFreezeListItemResponse> CreateAsync(string userId, CreateWeightFreezeRequest request);
        Task<WeightFreezeListItemResponse> TakeActionAsync(string adminUserId, long id, WeightFreezeActionRequest request);
        Task<WeightFreezeListItemResponse> UnfreezeAsync(string userId, bool isSuperAdmin, long id);
        Task<WeightFreezeImportResult> ImportCsvAsync(string userId, IFormFile file);
        (byte[] Content, string FileName) GetImportSampleCsv();
        Task<(byte[] Content, string FileName)> ExportCsvAsync(string userId, bool isSuperAdmin, WeightFreezeFilterRequest filter);
    }
}
