using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Common
{
    public  class BaseResponse<T>
    {
        public string? Message { get; set; }
        public T?  Data { get; set; }
        public string? StatusCode { get; set; }
        public bool? Success { get; set; }
    }
}
