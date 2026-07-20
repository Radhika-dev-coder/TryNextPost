namespace TryNextPost.Application.DTO.Shipment
{
    /// <summary>
    /// Query for seller shipment list. StatusTab mirrors NimbusPost shipment tabs.
    /// Accepted values (case-insensitive): all, Booked, PendingPickup, PickedUp/Picked,
    /// InTransit, OutForDelivery, Delivered, RTO, ReachedDestination, Exception, Cancelled, BookingFailed.
    /// </summary>
    public class ShipmentFilterRequest
    {
        public string? StatusTab { get; set; } = "all";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchQuery { get; set; }
    }
}
