namespace TryNextPost.Domain.Enums
{
    /// <summary>
    /// Shipment lifecycle statuses.
    /// Existing numeric values (1–6) are preserved for backward compatibility.
    /// Newer UI-aligned values start at 7.
    /// </summary>
    public enum ShipmentStatus
    {
        /// <summary>Legacy initial state (pre Phase-1 booking flow).</summary>
        Created = 1,

        /// <summary>Legacy pickup complete. Same value as <see cref="Picked"/>.</summary>
        PickedUp = 2,

        /// <summary>UI alias for <see cref="PickedUp"/>.</summary>
        Picked = 2,

        InTransit = 3,
        OutForDelivery = 4,
        Delivered = 5,
        RTO = 6,

        // ───── Phase 1 UI-aligned additions ─────
        Booked = 7,
        PendingPickup = 8,
        ReachedDestination = 9,
        Exception = 10,
        Cancelled = 11,
        BookingFailed = 12
    }
}
