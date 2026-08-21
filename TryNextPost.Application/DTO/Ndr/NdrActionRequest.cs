namespace TryNextPost.Application.DTO.Ndr
{
    public class NdrActionRequest
    {

        public long NdrId { get; set; }

        public string Action { get; set; } = string.Empty;
        public string? ActionType => Action; // Safe backup mapper

        public string? Remarks { get; set; }

        public DateTime? NextAttemptDate { get; set; }
    }
}
