using System.Text.RegularExpressions;

namespace LearnToCode.Services;

public sealed class SqlService
{
    public SqlAnalysisResult Analyze(string sql)
    {
        var result = new SqlAnalysisResult
        {
            OriginalSql = sql,
            FixedSql = sql
        };

        if (string.IsNullOrWhiteSpace(sql))
        {
            result.Issues.Add("No SQL query was provided.");
            result.Explanation = "Type a SQL statement so I can explain it.";
            return result;
        }

        if (!sql.TrimEnd().EndsWith(";"))
        {
            result.FixedSql = sql.TrimEnd() + ";";
            result.Issues.Add("Added a missing semicolon.");
        }

        result.FixedSql = Regex.Replace(result.FixedSql, @"\bSELET\b", "SELECT", RegexOptions.IgnoreCase);
        result.FixedSql = Regex.Replace(result.FixedSql, @"\bFRM\b", "FROM", RegexOptions.IgnoreCase);
        result.FixedSql = Regex.Replace(result.FixedSql, @"\bWHRE\b", "WHERE", RegexOptions.IgnoreCase);

        result.Explanation = "SQL tells a database what data to read or change. The parser needs exact keywords and a complete statement.";
        result.WhyWrong = result.Issues.Count > 0
            ? string.Join(" ", result.Issues)
            : "The query was readable, but the tutor still checked for beginner syntax problems.";
        result.NextTopics.AddRange(["SELECT basics", "Filtering with WHERE", "Joining tables", "Insert and update operations"]);
        return result;
    }
}

public sealed class SqlAnalysisResult
{
    public string OriginalSql { get; set; } = string.Empty;

    public string FixedSql { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;

    public string WhyWrong { get; set; } = string.Empty;

    public List<string> Issues { get; set; } = [];

    public List<string> NextTopics { get; set; } = [];
}
