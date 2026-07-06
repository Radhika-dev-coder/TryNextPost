using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Entities;

namespace TryNextPost.Domain.IRepository
{
    public interface IUserSessionRepository
    {
        Task AddAsync(UserSession session);
        Task SaveChangesAsync();
    }
}
