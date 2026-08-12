using TryNextPost.Domain.Enums;

namespace TryNextPost.Application.DTO.Settlement
{
    public class CourierSettlementResponse
    {
        public long CourierSettlementId { get; set; }
        public long CourierId { get; set; }
        public string CourierCode { get; set; } = string.Empty;
        public string CourierName { get; set; } = string.Empty;
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public decimal TotalCourierCost { get; set; }
        public decimal TotalSellerCharge { get; set; }
        public decimal TotalMargin { get; set; }
        public int ShipmentCount { get; set; }
        public SettlementStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime? SettledAt { get; set; }
        public string? PaymentReference { get; set; }
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<CourierSettlementLineResponse> Lines { get; set; } = [];
    }

    public class CourierSettlementLineResponse
    {
        public long CourierSettlementLineId { get; set; }
        public long ShipmentId { get; set; }
        public string? AwbNumber { get; set; }
        public decimal CourierCost { get; set; }
        public decimal SellerCharge { get; set; }
        public decimal Margin { get; set; }
        public DateTime? ShipmentBookedAt { get; set; }
    }
}
