namespace TryNextPost.Application.DTO.Ndr
{
    public class NdrFilterRequest
    {
        public string? StatusTab { get; set; } = "all";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchQuery { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}

