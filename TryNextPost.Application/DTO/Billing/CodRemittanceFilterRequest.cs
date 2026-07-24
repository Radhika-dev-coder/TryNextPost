namespace TryNextPost.Application.DTO.Billing
{
    public class CodRemittanceFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        /// <summary>all | pending | settled | failed</summary>
        public string? Status { get; set; } = "all";
    }
}
