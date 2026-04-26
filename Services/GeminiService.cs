using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LearnToCode.Models;

namespace LearnToCode.Services;

public sealed class GeminiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<GeminiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public bool IsEnabled()
    {
        return !string.IsNullOrWhiteSpace(GetApiKey());
    }

    public async Task<GeminiAnalysisResult?> TryAnalyzeAsync(
        CodeRequest request,
        ProgrammingLanguage language,
        CodeFixResult fallback,
        CancellationToken cancellationToken = default)
    {
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            return null;

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(GeminiService));

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var model = _configuration["Gemini:Model"] ?? "models/gemini-1.5-flash";
            var endpoint = $"https://generativelanguage.googleapis.com/v1/{model}:generateContent";

            var prompt = BuildPrompt(request, language, fallback);

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.2,
                    maxOutputTokens = 2048,
                    responseMimeType = "application/json"
                }
            };

            using var response = await client.PostAsync(
                endpoint,
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Gemini failed: {Status} {Error}", response.StatusCode, error);
                return null;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseResponse(body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GeminiService crashed");
            return null;
        }
    }

    private string? GetApiKey()
    {
        return _configuration["Gemini:ApiKey"]
            ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
    }

    // 🔥 FIXED PROMPT (NO RAW STRING BUGS)
    private static string BuildPrompt(
        CodeRequest request,
        ProgrammingLanguage language,
        CodeFixResult fallback)
    {
        var langName = LanguageDetector.ToDisplayName(language);

        var issues = fallback.Issues.Count == 0
            ? "No obvious issues detected"
            : string.Join(" | ", fallback.Issues);

        return $@"
You are LearnToCode AI Tutor.

You were created by Aarham.

Your job is to teach programming, debugging, and software development.

TASK:
- Explain code step-by-step
- Fix errors if needed
- Teach WHY errors happen
- Keep explanations simple and beginner friendly
- Focus ONLY on coding and software development

OUTPUT FORMAT (STRICT JSON ONLY):
{{
  ""detectedLanguage"": """",
  ""explanation"": """",
  ""whyWrong"": """",
  ""correctedCode"": """",
  ""lineByLine"": [],
  ""suggestions"": [],
  ""nextTopics"": [],
  ""tutorSummary"": """",
  ""executionOutput"": """"
}}

RULES:
- If code is correct, still explain it clearly
- Never go off-topic (always relate to programming)
- Be helpful like a coding tutor

USER CODE:
{request.Code}

DETECTED LANGUAGE:
{langName}

HEURISTIC ISSUES:
{issues}
";
    }

    private static GeminiAnalysisResult? ParseResponse(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);

            if (!doc.RootElement.TryGetProperty("candidates", out var candidates))
                return null;

            var text = candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(text))
                return null;

            return JsonSerializer.Deserialize<GeminiAnalysisResult>(text, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}