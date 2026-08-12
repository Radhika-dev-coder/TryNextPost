namespace TryNextPost.Application.DTO.Ndr
{
    public class NdrActionRequest
    {

        public string Action { get; set; } = "Reattempt";

        public string? Remarks { get; set; }

        public DateTime? NextAttemptDate { get; set; }
    }
}
