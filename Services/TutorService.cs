using LearnToCode.Models;

namespace LearnToCode.Services;

public sealed class TutorService
{
    public LessonModel BuildLesson(ProgrammingLanguage language, string code)
    {
        var topic = LanguageDetector.ToDisplayName(language);
        var lesson = new LessonModel
        {
            Title = $"{topic} learning mode",
            Summary = $"This lesson explains the code as if we are learning {topic} together.",
            ExampleCode = code
        };

        lesson.KeyIdeas.AddRange(BuildKeyIdeas(language));
        lesson.PracticePrompt = BuildPracticePrompt(language);
        return lesson;
    }

    public List<string> ExplainLineByLine(string code, ProgrammingLanguage language)
    {
        var lines = code.Split(Environment.NewLine, StringSplitOptions.None);
        var output = new List<string>();

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            output.Add($"{i + 1}. {ExplainLine(language, line)}");
        }

        if (output.Count == 0)
        {
            output.Add("Type code in the editor to get a step-by-step explanation.");
        }

        return output;
    }

    public List<string> BuildNextTopics(ProgrammingLanguage language)
    {
        return language switch
        {
            ProgrammingLanguage.Python => ["Variables", "Conditions", "Loops", "Functions"],
            ProgrammingLanguage.JavaScript => ["Variables", "DOM basics", "Functions", "Async programming"],
            ProgrammingLanguage.Sql => ["SELECT filters", "JOINs", "GROUP BY", "Data modification"],
            ProgrammingLanguage.Acl => ["Commands", "Variables", "Output", "Simple arithmetic"],
            ProgrammingLanguage.Html => ["Elements", "Attributes", "Forms", "Semantic layout"],
            ProgrammingLanguage.Css => ["Selectors", "Box model", "Layout", "Responsive design"],
            ProgrammingLanguage.C or ProgrammingLanguage.Cpp or ProgrammingLanguage.CSharp or ProgrammingLanguage.FSharp => ["Variables", "Conditions", "Loops", "Methods and functions"],
            _ => ["Variables", "Conditions", "Loops", "Functions"]
        };
    }

    private static IEnumerable<string> BuildKeyIdeas(ProgrammingLanguage language)
    {
        return language switch
        {
            ProgrammingLanguage.Python => ["Indentation tells Python where blocks begin and end.", "Colons usually introduce a new block.", "Function names must be spelled exactly."],
            ProgrammingLanguage.JavaScript => ["Semicolons are often helpful even when optional.", "Brackets and parentheses must be balanced.", "Console output helps with debugging."],
            ProgrammingLanguage.Sql => ["SQL commands must be complete and precise.", "SELECT reads data, INSERT adds data, UPDATE changes data, DELETE removes data.", "Statement endings matter."],
            ProgrammingLanguage.Acl => ["ACL commands are short and readable.", "Each command should be spelled exactly.", "The tutor can translate ACL into beginner-friendly steps."],
            ProgrammingLanguage.Html => ["HTML is about structure.", "Closing tags keep the page organized.", "Tags and attributes must be nested correctly."],
            ProgrammingLanguage.Css => ["CSS rules use selectors and declarations.", "Each property should end cleanly.", "Layout problems often come from missing braces or semicolons."],
            ProgrammingLanguage.C or ProgrammingLanguage.Cpp or ProgrammingLanguage.CSharp or ProgrammingLanguage.FSharp => ["Syntax is strict.", "Punctuation matters a lot.", "Blocks and function calls must be balanced."],
            _ => ["Code is read literally by the compiler or interpreter.", "Small typos matter.", "Learning the shape of the syntax helps you debug faster."]
        };
    }

    private static string BuildPracticePrompt(ProgrammingLanguage language)
    {
        return language switch
        {
            ProgrammingLanguage.Python => "Try writing a small program that prints your name and a number.",
            ProgrammingLanguage.JavaScript => "Try creating a variable and logging it to the console.",
            ProgrammingLanguage.Sql => "Try selecting all rows from a table with a WHERE filter.",
            ProgrammingLanguage.Acl => "Try storing a value and showing it back to the screen.",
            ProgrammingLanguage.Html => "Try creating a heading and a paragraph with one link.",
            ProgrammingLanguage.Css => "Try styling a card with padding, border, and background color.",
            _ => "Try writing a tiny example and then ask the tutor to explain it."
        };
    }

    private static string ExplainLine(ProgrammingLanguage language, string line)
    {
        if (line.StartsWith("//") || line.StartsWith("#"))
        {
            return "This is a comment, which helps humans understand the code.";
        }

        if (language == ProgrammingLanguage.Python && line.StartsWith("print"))
        {
            return "This prints text or values to the screen.";
        }

        if (language == ProgrammingLanguage.JavaScript && line.Contains("console.log"))
        {
            return "This sends a message to the browser console so you can inspect it while debugging.";
        }

        if (language == ProgrammingLanguage.Sql && line.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            return "This asks the database to return data from one or more tables.";
        }

        if (language == ProgrammingLanguage.Acl && line.StartsWith("show", StringComparison.OrdinalIgnoreCase))
        {
            return "This displays a value so the learner can see the result.";
        }

        if (line.Contains("="))
        {
            return "This likely stores or updates a value.";
        }

        if (line.Contains("(") && line.Contains(")"))
        {
            return "This looks like a function call or grouped expression.";
        }

        return "This line is part of the program logic and was checked for common beginner mistakes.";
    }
}
