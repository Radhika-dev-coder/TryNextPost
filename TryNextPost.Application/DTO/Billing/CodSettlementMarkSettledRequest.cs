namespace TryNextPost.Application.DTO.Billing
{
    public class CodSettlementMarkSettledRequest
    {
        public long CodSettlementId { get; set; }
        public string PaymentReference { get; set; } = string.Empty;
        public string? Remark { get; set; }
        public DateTime? SettlementDate { get; set; }
    }
}
