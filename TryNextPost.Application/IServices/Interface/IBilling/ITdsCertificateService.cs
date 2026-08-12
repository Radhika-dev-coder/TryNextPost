using Microsoft.AspNetCore.Http;
using TryNextPost.Application.DTO.Billing;

namespace TryNextPost.Application.IServices.Interface.IBilling
{
    public interface ITdsCertificateService
    {
        Task<TdsCertificateListResponse> GetForSellerAsync(string userId, TdsCertificateFilterRequest filter);

        Task<(byte[] Content, string FileName, string ContentType)> DownloadForSellerAsync(
            string userId,
            long tdsCertificateId);

        Task<TdsCertificateListResponse> GetForAdminAsync(TdsCertificateFilterRequest filter);

        Task<TdsCertificateListItemResponse> UploadForAdminAsync(
            string adminUserId,
            long sellerId,
            string financialYear,
            string quarter,
            string certificateNumber,
            decimal amount,
            string? deductorName,
            string? deductorTan,
            string? remarks,
            IFormFile file);

        Task<(byte[] Content, string FileName, string ContentType)> DownloadForAdminAsync(long tdsCertificateId);

        Task RevokeForAdminAsync(string adminUserId, long tdsCertificateId);

        Task<List<TdsSellerLookupDto>> GetSellerLookupAsync();
    }
}
