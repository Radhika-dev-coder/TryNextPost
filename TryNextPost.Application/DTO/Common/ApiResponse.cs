using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Application.DTO.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public List<string> Errors { get; set; }  
        public T Data { get; set; }

        public ApiStatusCode StatusCode { get; set; }
    }
}
