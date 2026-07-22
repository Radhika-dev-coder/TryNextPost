namespace TryNextPost.Application.DTO.Settlement
{
    public class MarkCourierSettlementPaidRequest
    {
        public long CourierSettlementId { get; set; }
        public string PaymentReference { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
