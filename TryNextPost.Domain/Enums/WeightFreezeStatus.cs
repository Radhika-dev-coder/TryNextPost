namespace TryNextPost.Domain.Enums
{
    public enum WeightFreezeStatus
    {
        Requested = 1,
        Accepted = 2,
        Rejected = 3,
        /// <summary>Previously accepted freeze deactivated — must not apply on book/rate.</summary>
        Unfrozen = 4
    }
}
