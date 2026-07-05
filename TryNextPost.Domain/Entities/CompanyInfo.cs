using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;


namespace TryNextPost.Domain.Entities
{
    public class CompanyInfo : BaseDbModel
    {
            [Key]
            public long CompanyId { get; set; } 
            public string Name { get; set; } = string.Empty;

            public ICollection<Seller>? Sellers { get; set; }
        public ICollection<Address> Addresses { get; set; }
    }
}
