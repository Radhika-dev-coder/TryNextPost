namespace TryNextPost.Application.DTO.Billing
{
    public class CodRemittanceListItemResponse
    {
        public long RemittanceId { get; set; }
        public long ShipmentId { get; set; }
        public string? AwbNumber { get; set; }
        public decimal CodAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal FreightDeductions { get; set; }
        public decimal RemittanceAmount { get; set; }
        public decimal ConvenienceFee { get; set; }
        public string? PaymentRef { get; set; }
        public string? Remark { get; set; }
    }
}
