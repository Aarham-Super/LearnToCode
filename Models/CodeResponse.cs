namespace LearnToCode.Models;

public sealed class CodeResponse
{
    public string DetectedLanguage { get; set; } = "Unknown";

    public string Action { get; set; } = "fix";

    public string OriginalCode { get; set; } = string.Empty;

    public string CorrectedCode { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;

    public string WhyWrong { get; set; } = string.Empty;

    public List<string> LineByLine { get; set; } = [];

    public List<string> Suggestions { get; set; } = [];

    public List<string> NextTopics { get; set; } = [];

    public string ExecutionOutput { get; set; } = string.Empty;

    public string TutorSummary { get; set; } = string.Empty;

    public UserProgress? Progress { get; set; }

    // ===================== NEW (IMPORTANT UPGRADES) =====================

    /// <summary>
    /// True if AI (Gemini) was used, false if fallback engine only
    /// </summary>
    public bool AiEnhanced { get; set; } = false;

    /// <summary>
    /// Status of response pipeline
    /// </summary>
    public string Status { get; set; } = "success"; // success | fallback | error

    /// <summary>
    /// Debug info (optional for dev mode)
    /// </summary>
    public string? DebugInfo { get; set; }

    /// <summary>
    /// Confidence score (0–100) of correctness
    /// </summary>
    public int Confidence { get; set; } = 100;
}