using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Domain.Entities
{
    public class Order : BaseDbModel
    {
        [Key]
        public long OrderId { get; set; }
        public long SellerId { get; set; }
        public Seller? Seller { get; set; }
        public string OrderRef { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal FinalPayableAmount { get; set; }
        public OrderCategoryEnum OrderCategory { get; set; } = OrderCategoryEnum.B2C;   // 👈 NAYA
        public PaymentMode PaymentMode { get; set; }
        public OrderTypeEnum OrderType { get; set; } = OrderTypeEnum.Forward;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Billing
        public long BillingAddressId { get; set; }
        public string? GstNumber { get; set; }   // 👈 NAYA

        // Shipping — embedded snapshot
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerCompanyName { get; set; }   // 👈 NAYA
        public string CustomerMobile { get; set; } = string.Empty;
        public string ShippingAddressLine1 { get; set; } = string.Empty;
        public string? ShippingAddressLine2 { get; set; }
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingState { get; set; } = string.Empty;
        public string ShippingPincode { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;

        // Package Info 

        [Column(TypeName = "decimal(18,2)")]
        public decimal WeightGrams { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LengthCm { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BreadthCm { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HeightCm { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal VolumetricWeightGrams { get; set; }

        // Charges — 👈 SAB NAYA

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCharges { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal CodCharges { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }
        public bool IsCollectableAmountDifferent { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal? CollectableAmount { get; set; }

        public ICollection<OrderItem>? OrderItems { get; set; }

    }
}

