namespace TryNextPost.Application.DTO.Shipment
{
    public class ShipmentListResponse
    {
        public List<ShipmentListItemResponse> Shipments { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public ShipmentTabCounts TabCounts { get; set; } = new();
    }
}
