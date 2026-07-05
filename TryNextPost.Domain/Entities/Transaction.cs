using System;
using System.ComponentModel.DataAnnotations;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities
{
    public class Transaction : BaseDbModel
    {
        [Key]
        public long TxnId { get; set; } 

        // 🔗 FK → Wallet
        public long WalletId { get; set; } 
        public Wallet? Wallet { get; set; }

        public TransactionType TxnType { get; set; } // Credit / Debit
        public decimal Amount { get; set; }           // ⚠ convention: always positive 
        public string? TxnReference { get; set; }     // Payment gateway / order ref
        public string? ReferenceId { get; set; }       // ShipmentId / OrderId etc.
        public string? Description { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    }
}