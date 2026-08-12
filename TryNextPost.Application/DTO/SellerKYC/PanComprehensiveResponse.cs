using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.SellerKYC
{
    public class PanComprehensiveResponse
    {
        public PanData? Data { get; set; }

        public int StatusCode { get; set; }

        public bool Success { get; set; }

        public string? Message { get; set; }

        public string? MessageCode { get; set; }
    }
}
