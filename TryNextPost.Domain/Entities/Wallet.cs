using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using TryNextPost.Domain.Common;

namespace TryNextPost.Domain.Entities
{
    public class Wallet : BaseDbModel
    {
        public string WalletId { get; set; } = Guid.NewGuid().ToString();

        // 🔗 FK → User (only ID, no navigation to Identity)
        public string UserId { get; set; } = string.Empty;

        public decimal Balance { get; set; } = 0;

        // Navigation
        public ICollection<Transaction>? Transactions { get; set; }
    }
}
