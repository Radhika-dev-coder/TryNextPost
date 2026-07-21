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

        public long SellerId { get; set; }
        public Seller? Seller { get; set; }

        // Legacy owner user id (audit); wallet is shared at SellerId level.
        public string UserId { get; set; } = string.Empty;

        public decimal Balance { get; set; } = 0;

        // Navigation
        public ICollection<Transaction>? Transactions { get; set; }
        public ICollection<WalletRecharge>? Recharges { get; set; }
    }
}