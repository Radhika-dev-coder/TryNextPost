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
            public string CompanyId { get; set; } = Guid.NewGuid().ToString();
            public string Name { get; set; } = string.Empty;
    }
}
