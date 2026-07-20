namespace TryNextPost.Application.DTO.Courier
{
    public class CourierRateResponse
    {
        public bool Success { get; set; }
        public bool IsStub { get; set; }
        public string CourierCode { get; set; } = string.Empty;
        public string? Message { get; set; }
        public List<CourierRateOption> Rates { get; set; } = new();
    }

    public class CourierRateOption
    {
        public string ServiceName { get; set; } = string.Empty;
        public string? ServiceCode { get; set; }
        public decimal TotalCharge { get; set; }
        public decimal? CodCharge { get; set; }
        public int EstimatedDays { get; set; }
        public bool IsStub { get; set; }
    }
}
