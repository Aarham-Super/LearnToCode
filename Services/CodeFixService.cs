using System.Text;
using System.Text.RegularExpressions;
using LearnToCode.Models;

namespace LearnToCode.Services;

public sealed class CodeFixService
{
    public CodeFixResult Fix(string code, ProgrammingLanguage language)
    {
        var result = new CodeFixResult
        {
            OriginalCode = code,
            FixedCode = code,
            Language = LanguageDetector.ToDisplayName(language)
        };

        if (string.IsNullOrWhiteSpace(code))
        {
            result.Issues.Add("The editor is empty, so there is nothing to analyze yet.");
            result.WhyWrong = "Begin by typing a small example, then we can explain it line by line.";
            result.Explanation = "I am waiting for code to help you with.";
            return result;
        }

        result.FixedCode = FixKnownTypos(result.FixedCode, language, result.Issues);
        result.FixedCode = BalancePunctuation(result.FixedCode, language, result.Issues);
        result.FixedCode = ApplyLanguageSpecificFixes(result.FixedCode, language, result.Issues);

        if (result.Issues.Count == 0)
        {
            result.Issues.Add("No obvious beginner syntax issue was detected.");
        }

        result.WhyWrong = BuildWhyWrong(language, result.Issues);
        result.Explanation = BuildExplanation(language, result.FixedCode);
        return result;
    }

    private static string FixKnownTypos(string code, ProgrammingLanguage language, List<string> issues)
    {
        var updated = code;
        var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["pritnt"] = language == ProgrammingLanguage.JavaScript ? "console.log" : "print",
            ["prinf"] = "print",
            ["consol.log"] = "console.log",
            ["syso"] = "System.out.println",
            ["SELET"] = "SELECT",
            ["FRM"] = "FROM",
            ["WHRE"] = "WHERE",
            ["shwo"] = "show"
        };

        foreach (var pair in replacements)
        {
            if (updated.Contains(pair.Key, StringComparison.OrdinalIgnoreCase))
            {
                updated = Regex.Replace(updated, Regex.Escape(pair.Key), pair.Value, RegexOptions.IgnoreCase);
                issues.Add($"Fixed the typo '{pair.Key}'.");
            }
        }

        return updated;
    }

    private static string BalancePunctuation(string code, ProgrammingLanguage language, List<string> issues)
    {
        var updated = code;
        updated = BalancePairs(updated, '(', ')', issues, "parentheses");
        updated = BalancePairs(updated, '{', '}', issues, "braces");
        updated = BalancePairs(updated, '[', ']', issues, "square brackets");
        updated = BalanceQuotes(updated, issues);

        if (language is ProgrammingLanguage.C or ProgrammingLanguage.Cpp or ProgrammingLanguage.CSharp or ProgrammingLanguage.JavaScript or ProgrammingLanguage.Sql or ProgrammingLanguage.Acl)
        {
            updated = EnsureSemicolons(updated, issues);
        }

        return updated;
    }

    private static string ApplyLanguageSpecificFixes(string code, ProgrammingLanguage language, List<string> issues)
    {
        return language switch
        {
            ProgrammingLanguage.Python => FixPython(code, issues),
            ProgrammingLanguage.Sql => FixSql(code, issues),
            ProgrammingLanguage.Acl => FixAcl(code, issues),
            ProgrammingLanguage.Html => FixHtml(code, issues),
            ProgrammingLanguage.Css => FixCss(code, issues),
            _ => code
        };
    }

    private static string FixPython(string code, List<string> issues)
    {
        var lines = code.Split('\n').Select(line =>
        {
            var trimmed = line.TrimEnd();
            if (Regex.IsMatch(trimmed, @"^\s*(if|elif|else|for|while|def|class)\b", RegexOptions.IgnoreCase) && !trimmed.EndsWith(':'))
            {
                issues.Add("Added a missing colon to a Python block.");
                return trimmed + ":";
            }

            return trimmed;
        }).ToArray();

        return string.Join(Environment.NewLine, lines);
    }

    private static string FixSql(string code, List<string> issues)
    {
        var updated = code;
        if (!updated.TrimEnd().EndsWith(";"))
        {
            updated = updated.TrimEnd() + ";";
            issues.Add("Added a missing SQL semicolon.");
        }

        updated = Regex.Replace(updated, @"\bSELET\b", "SELECT", RegexOptions.IgnoreCase);
        updated = Regex.Replace(updated, @"\bFRM\b", "FROM", RegexOptions.IgnoreCase);
        updated = Regex.Replace(updated, @"\bWHRE\b", "WHERE", RegexOptions.IgnoreCase);
        return updated;
    }

    private static string FixAcl(string code, List<string> issues)
    {
        var updated = code;
        if (!updated.TrimEnd().EndsWith(";"))
        {
            updated = updated.TrimEnd() + ";";
            issues.Add("Added a missing ACL terminator.");
        }

        updated = Regex.Replace(updated, @"\bshwo\b", "show", RegexOptions.IgnoreCase);
        updated = Regex.Replace(updated, @"\bprt\b", "print", RegexOptions.IgnoreCase);
        return updated;
    }

    private static string FixHtml(string code, List<string> issues)
    {
        if (!code.Contains("</html>", StringComparison.OrdinalIgnoreCase) && code.Contains("<html", StringComparison.OrdinalIgnoreCase))
        {
            issues.Add("Added a closing html tag.");
            return code + Environment.NewLine + "</html>";
        }

        return code;
    }

    private static string FixCss(string code, List<string> issues)
    {
        var lines = code.Split('\n').Select(line =>
        {
            var trimmed = line.TrimEnd();
            if (trimmed.Contains(':') && !trimmed.EndsWith(';') && !trimmed.EndsWith('{') && !trimmed.EndsWith('}'))
            {
                issues.Add("Added a missing CSS semicolon.");
                return trimmed + ";";
            }

            return trimmed;
        });

        return string.Join(Environment.NewLine, lines);
    }

    private static string BalancePairs(string code, char open, char close, List<string> issues, string label)
    {
        var opens = code.Count(c => c == open);
        var closes = code.Count(c => c == close);
        if (opens > closes)
        {
            issues.Add($"Added {opens - closes} missing {label}.");
            return code + new string(close, opens - closes);
        }

        return code;
    }

    private static string BalanceQuotes(string code, List<string> issues)
    {
        var single = code.Count(c => c == '\'');
        if (single % 2 == 1)
        {
            issues.Add("Balanced a missing single quote.");
            code += "'";
        }

        var dbl = code.Count(c => c == '"');
        if (dbl % 2 == 1)
        {
            issues.Add("Balanced a missing double quote.");
            code += "\"";
        }

        return code;
    }

    private static string EnsureSemicolons(string code, List<string> issues)
    {
        var lines = code.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].TrimEnd();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue;
            }

            if (trimmed.EndsWith('{') || trimmed.EndsWith('}') || trimmed.EndsWith(';') || trimmed.StartsWith("//") || trimmed.StartsWith("/*"))
            {
                continue;
            }

            if (Regex.IsMatch(trimmed, @"\b(if|for|while|switch|class|using|namespace|else)\b") && trimmed.Contains('{'))
            {
                continue;
            }

            if (!trimmed.EndsWith(';') && !trimmed.EndsWith(':'))
            {
                lines[i] = trimmed + ";";
                issues.Add("Added a missing semicolon.");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string BuildWhyWrong(ProgrammingLanguage language, IReadOnlyCollection<string> issues)
    {
        var summary = new StringBuilder();
        summary.Append($"This looks like {LanguageDetector.ToDisplayName(language)} code. ");
        summary.Append("The main problem is that beginner syntax rules were broken or a keyword was mistyped. ");
        summary.Append("That matters because compilers and interpreters read code very literally.");
        if (issues.Count > 0)
        {
            summary.Append(" Specific fixes: ");
            summary.Append(string.Join(" ", issues.Take(3)));
        }

        return summary.ToString();
    }

    private static string BuildExplanation(ProgrammingLanguage language, string code)
    {
        return language switch
        {
            ProgrammingLanguage.Python => "Python uses indentation and colons to mark code blocks. Small spelling mistakes can stop the whole program.",
            ProgrammingLanguage.JavaScript => "JavaScript is very strict about function names, quotes, and brackets, so one typo can break the script.",
            ProgrammingLanguage.C or ProgrammingLanguage.Cpp or ProgrammingLanguage.CSharp => "C-family languages need exact punctuation such as semicolons, braces, and parentheses.",
            ProgrammingLanguage.Sql => "SQL reads like a sentence. Keywords and statement endings matter because the database parser expects a precise structure.",
            ProgrammingLanguage.Acl => "ACL commands are short and direct. A wrong command word or missing terminator can change the meaning of the line.",
            ProgrammingLanguage.Html => "HTML describes page structure. Missing closing tags can make the browser guess the layout incorrectly.",
            ProgrammingLanguage.Css => "CSS works best when every declaration is clearly terminated. A missing semicolon can prevent styles from applying.",
            _ => "The code was analyzed for beginner-friendly syntax issues and the most likely mistakes were fixed."
        };
    }
}

public sealed class CodeFixResult
{
    public string Language { get; set; } = "Unknown";

    public string OriginalCode { get; set; } = string.Empty;

    public string FixedCode { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;

    public string WhyWrong { get; set; } = string.Empty;

    public List<string> Issues { get; set; } = [];
}
