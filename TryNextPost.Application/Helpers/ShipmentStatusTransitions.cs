using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

namespace TryNextPost.Application.Helpers
{
    /// <summary>
    /// Allowed shipment status transitions for booking / webhook / cancel flows.
    /// </summary>
    public static class ShipmentStatusTransitions
    {
        private static readonly Dictionary<ShipmentStatus, HashSet<ShipmentStatus>> Allowed =
            new()
            {
                [ShipmentStatus.Created] =
                [
                    ShipmentStatus.Booked,
                    ShipmentStatus.BookingFailed,
                    ShipmentStatus.Cancelled
                ],
                [ShipmentStatus.Booked] =
                [
                    ShipmentStatus.PendingPickup,
                    ShipmentStatus.PickedUp,
                    ShipmentStatus.Cancelled,
                    ShipmentStatus.Exception
                ],
                [ShipmentStatus.PendingPickup] =
                [
                    ShipmentStatus.PickedUp,
                    ShipmentStatus.Cancelled,
                    ShipmentStatus.Exception
                ],
                [ShipmentStatus.PickedUp] =
                [
                    ShipmentStatus.InTransit,
                    ShipmentStatus.OutForDelivery,
                    ShipmentStatus.Exception,
                    ShipmentStatus.RTO
                ],
                [ShipmentStatus.InTransit] =
                [
                    ShipmentStatus.ReachedDestination,
                    ShipmentStatus.OutForDelivery,
                    ShipmentStatus.Delivered,
                    ShipmentStatus.Exception,
                    ShipmentStatus.RTO
                ],
                [ShipmentStatus.ReachedDestination] =
                [
                    ShipmentStatus.OutForDelivery,
                    ShipmentStatus.Delivered,
                    ShipmentStatus.Exception,
                    ShipmentStatus.RTO
                ],
                [ShipmentStatus.OutForDelivery] =
                [
                    ShipmentStatus.Delivered,
                    ShipmentStatus.Exception,
                    ShipmentStatus.RTO,
                    ShipmentStatus.InTransit
                ],
                [ShipmentStatus.Exception] =
                [
                    ShipmentStatus.InTransit,
                    ShipmentStatus.OutForDelivery,
                    ShipmentStatus.Delivered,
                    ShipmentStatus.RTO,
                    ShipmentStatus.Cancelled
                ],
                [ShipmentStatus.Delivered] = [],
                [ShipmentStatus.RTO] = [],
                [ShipmentStatus.Cancelled] = [],
                [ShipmentStatus.BookingFailed] =
                [
                    ShipmentStatus.Booked,
                    ShipmentStatus.Cancelled
                ]
            };

        public static bool CanTransition(ShipmentStatus from, ShipmentStatus to)
        {
            if (from == to)
                return true;

            return Allowed.TryGetValue(from, out var next) && next.Contains(to);
        }

        public static void EnsureCanTransition(ShipmentStatus from, ShipmentStatus to)
        {
            if (!CanTransition(from, to))
            {
                throw new InvalidOperationException(
                    $"{SystemMessage.InvalidShipmentStatusTransition} ({from} → {to}).");
            }
        }

        public static bool IsCancellable(ShipmentStatus status)
        {
            return status is ShipmentStatus.Created
                or ShipmentStatus.Booked
                or ShipmentStatus.PendingPickup
                or ShipmentStatus.BookingFailed;
        }

        /// <summary>
        /// Maps courier / webhook status strings to <see cref="ShipmentStatus"/>.
        /// </summary>
        public static bool TryParseStatus(string? raw, out ShipmentStatus status)
        {
            status = default;
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            var normalized = raw.Trim()
                .Replace("-", "", StringComparison.Ordinal)
                .Replace("_", "", StringComparison.Ordinal)
                .Replace(" ", "", StringComparison.Ordinal);

            if (Enum.TryParse(normalized, ignoreCase: true, out ShipmentStatus parsed)
                && Enum.IsDefined(typeof(ShipmentStatus), parsed))
            {
                status = parsed;
                return true;
            }

            status = normalized.ToLowerInvariant() switch
            {
                "booked" or "manifested" or "created" => ShipmentStatus.Booked,
                "pendingpickup" or "pickuppending" or "scheduled" => ShipmentStatus.PendingPickup,
                "pickedup" or "picked" or "pickup" => ShipmentStatus.PickedUp,
                "intransit" or "transit" or "dispatched" => ShipmentStatus.InTransit,
                "reacheddestination" or "reachedhub" or "atdestination" => ShipmentStatus.ReachedDestination,
                "outofordelivery" or "ofd" => ShipmentStatus.OutForDelivery,
                "delivered" or "delivery" => ShipmentStatus.Delivered,
                "rto" or "returntoorigin" or "returned" => ShipmentStatus.RTO,
                "exception" or "undelivered" or "failed" or "ndr" => ShipmentStatus.Exception,
                "cancelled" or "canceled" => ShipmentStatus.Cancelled,
                "bookingfailed" => ShipmentStatus.BookingFailed,
                _ => (ShipmentStatus)(-1)
            };

            return (int)status >= 0;
        }
    }
}
