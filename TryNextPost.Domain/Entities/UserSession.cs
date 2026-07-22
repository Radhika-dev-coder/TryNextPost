using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Entities
{
    public class UserSession
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string DeviceId { get; set; }

        public string IpAddress { get; set; }

        public string JwtToken { get; set; }

        public string? RefreshTokenHash { get; set; }

        public DateTime? RefreshTokenExpiryAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ExpiryAt { get; set; }

        public bool IsActive { get; set; }
    }
}
