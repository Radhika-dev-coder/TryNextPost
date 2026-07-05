using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Auth
{
    public class IdentityResultModel
    {
        public bool Succeeded { get; set; }
        public string UserId { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
    }
}
