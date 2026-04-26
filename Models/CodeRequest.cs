namespace LearnToCode.Models;

public sealed class CodeRequest
{
    // The code user writes in Monaco editor
    public string Code { get; set; } = string.Empty;

    // Optional hint from UI dropdown
    public string? LanguageHint { get; set; }

    // Action: fix | explain | learn
    public string Action { get; set; } = "fix";

    // Used for progress tracking
    public string UserId { get; set; } = "default";

    // Optional: enables deeper tutor explanations
    public bool LearnMode { get; set; } = true;

    // Optional: future feature (AI auto-suggestions while typing)
    public bool AutoHelpEnabled { get; set; } = true;

    // Optional: topic (SQL, loops, arrays, etc.)
    public string? Topic { get; set; }
}