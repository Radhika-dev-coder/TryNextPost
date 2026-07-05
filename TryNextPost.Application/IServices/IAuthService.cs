using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Auth;
using TryNextPost.Application.DTO.Common;

namespace TryNextPost.Application.IServices
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResult>> RegisterAsync(SellerDto dto);


    }
}
