using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Auth
{
    public class AuthResult
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }     // JWT
        public string Role { get; set; }
    }
}
