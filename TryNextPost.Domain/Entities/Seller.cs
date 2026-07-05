using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities
{
    public class Seller : BaseDbModel
    {
        [Key]
        public long SellerId { get; set; } 

        public string UserId { get; set; } = string.Empty;

        public long? CompanyId { get; set; }
        public CompanyInfo? Company { get; set; }

        public string? GstNumber { get; set; }
        public SellerStatus Status { get; set; } = SellerStatus.Active;

 
    }
}
