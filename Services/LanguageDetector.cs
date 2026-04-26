using System.Text.RegularExpressions;

namespace LearnToCode.Services;

public enum ProgrammingLanguage
{
    Unknown,
    Html,
    Css,
    JavaScript,
    Python,
    C,
    Cpp,
    CSharp,
    FSharp,
    Sql,
    Acl
}

public sealed class LanguageDetector
{
    public ProgrammingLanguage Detect(string code, string? hint = null)
    {
        if (!string.IsNullOrWhiteSpace(hint) &&
            Enum.TryParse<ProgrammingLanguage>(NormalizeHint(hint), true, out var hinted))
        {
            return hinted;
        }

        var sample = code?.Trim() ?? "";
        if (sample.Length == 0)
            return ProgrammingLanguage.Unknown;

        // -------------------------
        // HTML
        // -------------------------
        if (Regex.IsMatch(sample, @"<\s*(!doctype|html|head|body|div|span|script|style)\b", RegexOptions.IgnoreCase))
        {
            return ProgrammingLanguage.Html;
        }

        // -------------------------
        // CSS (FIXED + STRONG)
        // -------------------------
        if (
            Regex.IsMatch(sample, @"[.#a-zA-Z0-9_-]+\s*\{") &&
            sample.Contains(":") &&
            sample.Contains("}")
        )
        {
            return ProgrammingLanguage.Css;
        }

        // extra CSS safety net
        if (sample.Contains("margin") ||
            sample.Contains("padding") ||
            sample.Contains("color:") ||
            sample.Contains("background"))
        {
            return ProgrammingLanguage.Css;
        }

        // -------------------------
        // JavaScript
        // -------------------------
        if (Regex.IsMatch(sample, @"\b(console\.log|function|let|const|=>|document\.|window\.)\b", RegexOptions.IgnoreCase))
        {
            return ProgrammingLanguage.JavaScript;
        }

        // -------------------------
        // Python
        // -------------------------
        if (Regex.IsMatch(sample, @"\b(def|import|print|self|elif|None|True|False)\b", RegexOptions.IgnoreCase))
        {
            return ProgrammingLanguage.Python;
        }

        // -------------------------
        // C
        // -------------------------
        if (Regex.IsMatch(sample, @"\b(#include|printf\s*\(|scanf\s*\(|int\s+main\s*\()"))
        {
            return ProgrammingLanguage.C;
        }

        // -------------------------
        // C++
        // -------------------------
        if (Regex.IsMatch(sample, @"\b(std::|cout\s*<<|cin\s*>>|class\s+\w+|template\s*<)"))
        {
            return ProgrammingLanguage.Cpp;
        }

        // -------------------------
        // C#
        // -------------------------
        if (Regex.IsMatch(sample, @"\b(Console\.WriteLine|using\s+System|namespace\s+\w+|var\s+\w+\s*=|async\s+Task)\b"))
        {
            return ProgrammingLanguage.CSharp;
        }

        // -------------------------
        // F#
        // -------------------------
        if (Regex.IsMatch(sample, @"\b(open\s+System|let\s+\w+\s*=|printfn|module\s+\w+)\b", RegexOptions.IgnoreCase))
        {
            return ProgrammingLanguage.FSharp;
        }

        // -------------------------
        // SQL
        // -------------------------
        if (LooksLikeSql(sample))
        {
            return ProgrammingLanguage.Sql;
        }

        // -------------------------
        // ACL
        // -------------------------
        if (LooksLikeAcl(sample))
        {
            return ProgrammingLanguage.Acl;
        }

        // IMPORTANT FIX:
        // DO NOT default to Python (this caused your bug before)
        return ProgrammingLanguage.Unknown;
    }

    public static string ToDisplayName(ProgrammingLanguage language) => language switch
    {
        ProgrammingLanguage.Html => "HTML",
        ProgrammingLanguage.Css => "CSS",
        ProgrammingLanguage.JavaScript => "JavaScript",
        ProgrammingLanguage.Python => "Python",
        ProgrammingLanguage.C => "C",
        ProgrammingLanguage.Cpp => "C++",
        ProgrammingLanguage.CSharp => "C#",
        ProgrammingLanguage.FSharp => "F#",
        ProgrammingLanguage.Sql => "SQL",
        ProgrammingLanguage.Acl => "ACL",
        _ => "Unknown"
    };

    private static string NormalizeHint(string hint)
    {
        return hint.Trim()
            .Replace("c#", "CSharp", StringComparison.OrdinalIgnoreCase)
            .Replace("c++", "Cpp", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeSql(string sample)
    {
        return Regex.IsMatch(sample,
            @"\b(SELECT|INSERT|UPDATE|DELETE|FROM|WHERE|JOIN|GROUP\s+BY|ORDER\s+BY|CREATE\s+TABLE)\b",
            RegexOptions.IgnoreCase);
    }

    private static bool LooksLikeAcl(string sample)
    {
        return Regex.IsMatch(sample,
                   @"\b(set|show|add|sub|mul|div|print|echo)\b",
                   RegexOptions.IgnoreCase)
               && !Regex.IsMatch(sample, @"[;{}<>]");
    }
}