namespace TryNextPost.Application.DTO.Settlement
{
    public class CreateCourierSettlementRequest
    {
        public long CourierId { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public string? Notes { get; set; }
    }
}
