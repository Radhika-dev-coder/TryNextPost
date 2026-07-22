namespace TryNextPost.Application.DTO.Weight
{
    public class WeightFreezeActionRequest
    {
        /// <summary>Accept or Reject.</summary>
        public string Action { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }
}
