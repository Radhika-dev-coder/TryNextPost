using System.ComponentModel.DataAnnotations;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities
{
    /// <summary>
    /// Razorpay payment order for wallet top-up. Credit happens only after Paid (webhook / verify).
    /// </summary>
    public class WalletRecharge : BaseDbModel
    {
        [Key]
        public long WalletRechargeId { get; set; }

        /// <summary>AspNet Identity user id (same key as Wallet.UserId).</summary>
        public string UserId { get; set; } = string.Empty;

        public long WalletId { get; set; }
        public Wallet? Wallet { get; set; }

        /// <summary>Amount in rupees (e.g. 100.00).</summary>
        public decimal Amount { get; set; }

        /// <summary>Amount in paise for Razorpay Orders API (e.g. 10000).</summary>
        public int AmountInPaise { get; set; }

        public string Currency { get; set; } = "INR";

        /// <summary>Razorpay order id (order_xxx).</summary>
        public string GatewayOrderId { get; set; } = string.Empty;

        /// <summary>Razorpay payment id (pay_xxx), set after capture.</summary>
        public string? GatewayPaymentId { get; set; }

        public WalletRechargeStatus Status { get; set; } = WalletRechargeStatus.Pending;

        /// <summary>Receipt passed to Razorpay (unique per attempt).</summary>
        public string Receipt { get; set; } = string.Empty;
    }
}
