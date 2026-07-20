using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Order
{
    public class CreateReverseQcOrderRequest : CreateOrderRequestBase
    {
        [Required]
        public string ProductCategory { get; set; } = string.Empty;
        public bool? IsUsedProduct { get; set; }
        public bool? IsDamagedProduct { get; set; }
        public bool? IsBrandMatched { get; set; }
        public bool? IsSizeMatched { get; set; }
        public bool? IsColorMatched { get; set; }
        [MinLength(1, ErrorMessage = "At least one reference image is required")]
        public List<string> ReferenceImageUrls { get; set; } = new();
    }
}
