namespace TryNextPost.Domain.Enums
{

    public enum ShipmentStatus
    {
        Created = 1,
        Booked = 2,             
        PendingPickup = 3,
        PickupScheduled = 4,
        PickedUp = 5,
        InTransit = 6,
        ReachedDestination = 7,
        OutForDelivery = 8,
        Delivered = 9,

        DeliveryAttemptFailed = 10,
        NDR = 11,

        RTOInitiated = 12,
        RTOInTransit = 13,
        RTODelivered = 14,
        RTOAcknowledged = 15,    

        Exception = 16,
        Lost = 17,
        Damaged = 18,
        Cancelled = 19,
        BookingFailed = 20
    }
}
