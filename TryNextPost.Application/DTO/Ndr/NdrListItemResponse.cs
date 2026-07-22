namespace TryNextPost.Application.DTO.Ndr
{
    public class NdrListItemResponse
    {
        public long NdrId { get; set; }
        public long ShipmentId { get; set; }
        public long OrderId { get; set; }
        public string Channel { get; set; } = string.Empty;
        public DateTime? NdrDate { get; set; }
        public string OrderRef { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string Payment { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Carrier { get; set; } = string.Empty;
        public string? Awb { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? Action { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public int Attempts { get; set; }
        public DateTime? NextAttemptDate { get; set; }
    }
}
