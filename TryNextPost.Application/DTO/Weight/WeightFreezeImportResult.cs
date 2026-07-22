namespace TryNextPost.Application.DTO.Weight
{
    public class WeightFreezeImportResult
    {
        public int ImportedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
