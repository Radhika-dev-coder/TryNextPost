using System.ComponentModel.DataAnnotations;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class PincodeZoneMapping : BaseDbModel
    {
        [Key]
        public int PincodeZoneMappingId { get; set; }

        /// <summary>First 2 digits of Indian pincode (00–99).</summary>
        [MaxLength(2)]
        public string PincodePrefix { get; set; } = string.Empty;

        public int ZoneId { get; set; }
        public Zone? Zone { get; set; }
    }
}
