namespace LearnToCode.Models;

public sealed class GeminiAnalysisResult
{
    public string DetectedLanguage { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;

    public string WhyWrong { get; set; } = string.Empty;

    public string CorrectedCode { get; set; } = string.Empty;

    public List<string> LineByLine { get; set; } = new();

    public List<string> Suggestions { get; set; } = new();

    public List<string> NextTopics { get; set; } = new();

    public string TutorSummary { get; set; } = string.Empty;

    public string ExecutionOutput { get; set; } = string.Empty;
}