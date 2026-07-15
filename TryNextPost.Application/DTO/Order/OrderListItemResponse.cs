using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryNextPost.Application.DTO.Order
{
    public class OrderListItemResponse
    {
        public long OrderId { get; set; }
        public string Channel { get; set; } = "Manual";
        public string OrderRef { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string ProductSummary { get; set; } = string.Empty;
        public string PaymentMode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerMobile { get; set; } = string.Empty;
        public decimal WeightGrams { get; set; }
        public string? IvrStatus { get; set; }
        public string? WhatsAppStatus { get; set; }
        public string? ShopifyTags { get; set; }
        public string? Tags { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
