namespace TryNextPost.Application.DTO.Billing
{
    public class CodRemittanceSummaryResponse
    {
        public decimal RemittedTillDate { get; set; }
        public decimal LastRemittance { get; set; }
        public decimal NextRemittanceExpected { get; set; }
        public decimal TotalRemittanceDue { get; set; }
    }
}
