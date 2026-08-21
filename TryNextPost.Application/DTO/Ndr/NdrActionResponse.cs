using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Ndr
{
    public sealed class NdrActionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? UpdatedStatusName { get; set; }
    }
}
