using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.IServices.Interface
{
    public interface IAmazonAuthService
    {
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    }
}
