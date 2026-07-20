namespace TryNextPost.Application.DTO.Shipment
{
    public class ShipmentTabCounts
    {
        public int All { get; set; }
        public int Booked { get; set; }
        public int PendingPickup { get; set; }
        public int PickedUp { get; set; }
        public int InTransit { get; set; }
        public int OutForDelivery { get; set; }
        public int Delivered { get; set; }
        public int Rto { get; set; }
        public int Exception { get; set; }
        public int Cancelled { get; set; }
    }
}
