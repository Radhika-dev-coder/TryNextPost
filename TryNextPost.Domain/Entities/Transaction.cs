using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities
{
    public class Transaction : BaseDbModel
    {
        public string TxnId { get; set; } = Guid.NewGuid().ToString();

        // 🔗 FK → Wallet
        public string WalletId { get; set; } = string.Empty;

        public TransactionType TxnType { get; set; } // Credit / Debit

        public decimal Amount { get; set; }

        public string? TxnReference { get; set; } // Payment gateway / order ref

        public string? ReferenceId { get; set; } // ShipmentId / OrderId etc.

        public string? Description { get; set; }

        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;


        // Navigation
        public Wallet? Wallet { get; set; }
    }
}
