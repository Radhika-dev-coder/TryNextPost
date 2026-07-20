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
<<<<<<< Updated upstream
        public OrderCategoryEnum OrderCategory { get; set; } = OrderCategoryEnum.B2C;   
=======
        public OrderCategoryEnum OrderCategory { get; set; } = OrderCategoryEnum.B2C;  
>>>>>>> Stashed changes
        public PaymentMode PaymentMode { get; set; }
        public OrderTypeEnum OrderType { get; set; } = OrderTypeEnum.Forward;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

<<<<<<< Updated upstream
        
=======
>>>>>>> Stashed changes
        public string? GstNumber { get; set; }

        public long? PickupAddressId { get; set; }
        public Address? PickupAddress { get; set; }
<<<<<<< Updated upstream

        public bool IsBillingSameAsShipping { get; set; } = true;
        public string? BillingFirstName { get; set; }
        public string? BillingLastName { get; set; }
        public string? BillingCompanyName { get; set; }
        public string? BillingAddressLine1 { get; set; }
        public string? BillingAddressLine2 { get; set; }
        public string? BillingCity { get; set; }
        public string? BillingState { get; set; }
        public string? BillingPincode { get; set; }
        public string? BillingCountry { get; set; }
=======
>>>>>>> Stashed changes

        // Shipping — embedded snapshot
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerCompanyName { get; set; }   
        public string CustomerMobile { get; set; } = string.Empty;
        public string ShippingAddressLine1 { get; set; } = string.Empty;
        public string? ShippingAddressLine2 { get; set; }
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingState { get; set; } = string.Empty;
        public string ShippingPincode { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;

        //Billing 

        public bool IsBillingSameAsShipping { get; set; } = true;
        public string? BillingFirstName { get; set; }
        public string? BillingLastName { get; set; }
        public string? BillingCompanyName { get; set; }
        public string? BillingAddressLine1 { get; set; }
        public string? BillingAddressLine2 { get; set; }
        public string? BillingCity { get; set; }
        public string? BillingState { get; set; }
        public string? BillingPincode { get; set; }
        public string? BillingCountry { get; set; }

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

        // Charges — 

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

        public ICollection<OrderItem> OrderItems { get; set; }
      = new List<OrderItem>();

        public ReverseQcDetail? ReverseQcDetail { get; set; }


        public string Channel { get; set; } = "Manual";
        public string? IvrStatus { get; set; }
        public string? WhatsAppStatus { get; set; }
        public string? ShopifyTags { get; set; }
        public string? Tags { get; set; }

    }
}

