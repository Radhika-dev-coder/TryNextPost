using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;

namespace TryNextPost.Infrastructure.Repository
{
    public class UserSessionRepository : IUserSessionRepository
    {
        private readonly AppDbContext _context;

        public UserSessionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserSession session)
        {
            await _context.UserSessions.AddAsync(session);
        }

        public Task<UserSession?> GetByIdAsync(int sessionId)
        {
            return _context.UserSessions.FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public Task<UserSession?> GetByRefreshTokenHashAsync(string refreshTokenHash)
        {
            return _context.UserSessions.FirstOrDefaultAsync(s =>
                s.RefreshTokenHash == refreshTokenHash && s.IsActive);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
