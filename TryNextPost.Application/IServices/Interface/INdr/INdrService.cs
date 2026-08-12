using TryNextPost.Application.DTO.Ndr;

namespace TryNextPost.Application.IServices.Interface.INdr
{
    public interface INdrService
    {
        Task<NdrListResponse> GetListAsync(string userId, NdrFilterRequest filter);
        Task<NdrListItemResponse> TakeActionAsync(string userId, long ndrId, NdrActionRequest request);
    }
}
