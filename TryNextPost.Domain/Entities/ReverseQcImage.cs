using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Entities
{
    public class ReverseQcImage
    {

        [Key]
        public long ReverseQcImageId { get; set; }
        public long ReverseQcDetailId { get; set; }
        public ReverseQcDetail ReverseQcDetail { get; set; } = null!;
        [MaxLength(1000)]
        public string ImageUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }
}
