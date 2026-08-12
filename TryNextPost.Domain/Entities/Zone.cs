using System.ComponentModel.DataAnnotations;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class Zone : BaseDbModel
    {
        [Key]
        public int ZoneId { get; set; }

        [MaxLength(10)]
        public string ZoneCode { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ZoneName { get; set; } = string.Empty;

        public ICollection<PincodeZoneMapping>? PincodeMappings { get; set; }
    }
}
