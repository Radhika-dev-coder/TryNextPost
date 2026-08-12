using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Domain.Entities
{
    public class Region
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string PincodePrefix { get; set; }

        public int ZoneId { get; set; }
        public Zone Zone { get; set; }
    }
}
