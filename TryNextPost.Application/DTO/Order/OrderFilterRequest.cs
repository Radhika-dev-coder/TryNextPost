using TryNextPost.Domain.Enums;

namespace TryNextPost.Application.DTO.Order
{
    public class OrderFilterRequest
    {
        /// <summary>When null, defaults to B2C for backward-compatible seller lists.</summary>
        public OrderCategoryEnum? OrderCategory { get; set; }

        public string? Tab { get; set; } = "all";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? OrderIds { get; set; }
        public string? SearchQuery { get; set; }
        public string? ProductName { get; set; }
        public string? Channel { get; set; }

        //COD | Prepaid | Forward | Reverse | ReverseQC
        public string? Type { get; set; }

        //Explicit: Forward | Reverse | ReverseQC
        public string? OrderType { get; set; }
        public string? IvrStatus { get; set; }
        public string? WhatsAppStatus { get; set; }
        public string? Tags { get; set; }
    }
}
