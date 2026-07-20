using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class Otp : BaseDbModel
    {
        public long OtpId { get; set; }
        [MaxLength(15)]
        public string MobileNumber { get; set; } = string.Empty;
        [MaxLength(64)]
        public string CodeHash { get; set; } = string.Empty;
        public DateTime ExpiryTime { get; set; }
        public bool IsUsed { get; set; }
        public int FailedAttempts { get; set; }
    }
}
