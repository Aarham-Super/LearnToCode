namespace LearnToCode.Models;

public sealed class UserProgress
{
    public string UserId { get; set; } = "default";

    public int TotalSubmissions { get; set; }

    public int FixedSubmissions { get; set; }

    public string LastLanguage { get; set; } = "Unknown";

    public DateTimeOffset LastUpdatedUtc { get; set; } = DateTimeOffset.UtcNow;

    public Dictionary<string, int> MistakeCounts { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public List<string> RecentMistakes { get; set; } = [];

    public List<string> LearnedTopics { get; set; } = [];
}
