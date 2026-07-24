namespace TryNextPost.Application.DTO.Billing
{
    public class InvoiceListItemResponse
    {
        public long InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public string InvoicePeriod { get; set; } = string.Empty;
        public decimal InvoiceAmount { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
    }
}
