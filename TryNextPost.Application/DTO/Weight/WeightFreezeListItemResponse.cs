namespace TryNextPost.Application.DTO.Weight
{
    public class WeightFreezeListItemResponse
    {
        public long ProductWeightFreezeId { get; set; }

        /// <summary>Owning seller — required for SuperAdmin multi-seller table.</summary>
        public long SellerId { get; set; }

        /// <summary>Company / business name when available.</summary>
        public string SellerName { get; set; } = string.Empty;

        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public decimal LengthCm { get; set; }
        public decimal BreadthCm { get; set; }
        public decimal HeightCm { get; set; }
        public string Dimensions { get; set; } = string.Empty;
        public decimal WeightGrams { get; set; }
        public bool AutoApply { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? ActionRemarks { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
