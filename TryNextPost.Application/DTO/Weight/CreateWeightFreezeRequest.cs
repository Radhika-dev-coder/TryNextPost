namespace TryNextPost.Application.DTO.Weight
{
    public class CreateWeightFreezeRequest
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public decimal LengthCm { get; set; }
        public decimal BreadthCm { get; set; }
        public decimal HeightCm { get; set; }
        public decimal WeightGrams { get; set; }
        public bool AutoApply { get; set; } = true;
    }
}
