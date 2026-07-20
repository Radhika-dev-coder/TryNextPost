using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Entities
{
    public class ReverseQcDetail
    {
        [Key]
        public long ReverseQcDetailId { get; set; }
        public long OrderId { get; set; }
        public Order Order { get; set; } = null!;
        [MaxLength(100)]
        public string ProductCategory { get; set; } = string.Empty;
 
        public bool? IsUsedProduct { get; set; }
        public bool? IsDamagedProduct { get; set; }
        public bool? IsBrandMatched { get; set; }
        public bool? IsSizeMatched { get; set; }
        public bool? IsColorMatched { get; set; }
        public ICollection<ReverseQcImage> Images { get; set; }
            = new List<ReverseQcImage>();
    }
}
