using System.Diagnostics;
using System.Text;

namespace LearnToCode.Services;

public sealed class AclService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AclService> _logger;

    public AclService(IConfiguration configuration, ILogger<AclService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AclAnalysisResult> AnalyzeAsync(string code, CancellationToken cancellationToken = default)
    {
        var rootPath = _configuration["AclEngine:RootPath"] ?? @"C:\AARHAM\ACL";
        var result = new AclAnalysisResult
        {
            OriginalCode = code,
            FixedCode = code,
            EnginePath = rootPath
        };

        if (string.IsNullOrWhiteSpace(code))
        {
            result.Explanation = "Type ACL code and I will explain it like a tutor.";
            result.WhyWrong = "There is no ACL command to analyze yet.";
            return result;
        }

        result.FixedCode = code.TrimEnd();
        if (!result.FixedCode.EndsWith(';'))
        {
            result.FixedCode += ";";
            result.Issues.Add("Added a missing ACL terminator.");
        }

        result.Explanation = "ACL is treated as a small beginner language. The tutor looks for simple command errors and explains the intent line by line.";
        result.WhyWrong = result.Issues.Count > 0 ? string.Join(" ", result.Issues) : "No obvious ACL syntax issue was found.";

        var engineOutput = await TryRunExternalEngineAsync(rootPath, code, cancellationToken);
        if (!string.IsNullOrWhiteSpace(engineOutput))
        {
            result.ExecutionOutput = engineOutput;
        }

        result.NextTopics.AddRange(["Variables", "Showing values", "Simple math", "If/then style logic"]);
        return result;
    }

    private async Task<string> TryRunExternalEngineAsync(string rootPath, string code, CancellationToken cancellationToken)
    {
        try
        {
            if (!Directory.Exists(rootPath))
            {
                return "ACL engine path was not found, so execution is simulated inside LearnToCode.";
            }

            var candidates = new[]
            {
                Path.Combine(rootPath, "acl.exe"),
                Path.Combine(rootPath, "engine.exe"),
                Path.Combine(rootPath, "run.bat"),
                Path.Combine(rootPath, "run.cmd"),
                Path.Combine(rootPath, "acl.ps1")
            };

            var executable = candidates.FirstOrDefault(File.Exists);
            if (string.IsNullOrWhiteSpace(executable))
            {
                return "No runnable ACL engine file was found in C:\\AARHAM\\ACL, so LearnToCode is using a teaching fallback.";
            }

            var psi = new ProcessStartInfo
            {
                FileName = executable,
                WorkingDirectory = rootPath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (executable.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) || executable.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase))
            {
                psi.FileName = "cmd.exe";
                psi.Arguments = $"/c \"{executable}\"";
            }
            else if (executable.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
            {
                psi.FileName = "powershell.exe";
                psi.Arguments = $"-ExecutionPolicy Bypass -File \"{executable}\"";
            }

            using var process = Process.Start(psi);
            if (process is null)
            {
                return "ACL engine could not start, so the response was simulated.";
            }

            await process.StandardInput.WriteLineAsync(code);
            process.StandardInput.Close();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync(cancellationToken);

            var combined = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(output))
            {
                combined.AppendLine(output.Trim());
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                combined.AppendLine(error.Trim());
            }

            return combined.ToString().Trim();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ACL engine execution failed");
            return "ACL execution could not complete, so LearnToCode returned a safe fallback explanation.";
        }
    }
}

public sealed class AclAnalysisResult
{
    public string OriginalCode { get; set; } = string.Empty;

    public string FixedCode { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;

    public string WhyWrong { get; set; } = string.Empty;

    public string EnginePath { get; set; } = string.Empty;

    public string ExecutionOutput { get; set; } = string.Empty;

    public List<string> NextTopics { get; set; } = [];

    public List<string> Issues { get; set; } = [];
}
