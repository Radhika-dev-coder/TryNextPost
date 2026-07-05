using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class Wallet : BaseDbModel
    {
        [Key]
        public long WalletId { get; set; } 

        // 🔗 FK → User (only ID, no navigation to Identity)
        public string UserId { get; set; } = string.Empty;

        public decimal Balance { get; set; } = 0;

        // Navigation
        public ICollection<Transaction>? Transactions { get; set; }
    }
}