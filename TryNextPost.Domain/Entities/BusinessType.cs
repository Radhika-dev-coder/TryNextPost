using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class BusinessType : BaseDbModel
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set;}
    }
}
