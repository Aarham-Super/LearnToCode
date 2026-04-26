namespace LearnToCode.Models;

public sealed class LessonModel
{
    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public List<string> KeyIdeas { get; set; } = [];

    public string ExampleCode { get; set; } = string.Empty;

    public string PracticePrompt { get; set; } = string.Empty;
}
