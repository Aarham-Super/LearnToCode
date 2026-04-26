using LearnToCode.Models;

namespace LearnToCode.Services;

public sealed class AIService
{
    private readonly LanguageDetector _languageDetector;
    private readonly CodeFixService _codeFixService;
    private readonly TutorService _tutorService;
    private readonly SqlService _sqlService;
    private readonly AclService _aclService;
    private readonly GeminiService _geminiService;
    private readonly LearningProgressStore _progressStore;

    public AIService(
        LanguageDetector languageDetector,
        CodeFixService codeFixService,
        TutorService tutorService,
        SqlService sqlService,
        AclService aclService,
        GeminiService geminiService,
        LearningProgressStore progressStore)
    {
        _languageDetector = languageDetector;
        _codeFixService = codeFixService;
        _tutorService = tutorService;
        _sqlService = sqlService;
        _aclService = aclService;
        _geminiService = geminiService;
        _progressStore = progressStore;
    }

    public async Task<CodeResponse> AnalyzeAsync(CodeRequest request, CancellationToken cancellationToken = default)
    {
        var language = _languageDetector.Detect(request.Code, request.LanguageHint);

        var response = new CodeResponse
        {
            DetectedLanguage = LanguageDetector.ToDisplayName(language),
            Action = request.Action,
            OriginalCode = request.Code
        };

        // SQL
        if (language == ProgrammingLanguage.Sql)
        {
            var sql = _sqlService.Analyze(request.Code);

            response.CorrectedCode = sql.FixedSql;
            response.Explanation = sql.Explanation;
            response.WhyWrong = sql.WhyWrong;
            response.Suggestions = sql.Issues;
            response.NextTopics = sql.NextTopics;
            response.LineByLine = _tutorService.ExplainLineByLine(response.CorrectedCode, language);
            response.TutorSummary = "SQL mode: explains query execution and syntax rules.";

            response.Progress = await _progressStore.RecordAsync(
                request.UserId,
                response.DetectedLanguage,
                sql.Issues);

            return await MergeGeminiAsync(request, language, response, request.Code, sql.Issues, cancellationToken);
        }

        // ACL
        if (language == ProgrammingLanguage.Acl)
        {
            var acl = await _aclService.AnalyzeAsync(request.Code, cancellationToken);

            response.CorrectedCode = acl.FixedCode;
            response.Explanation = acl.Explanation;
            response.WhyWrong = acl.WhyWrong;
            response.ExecutionOutput = acl.ExecutionOutput;
            response.Suggestions = acl.Issues;
            response.NextTopics = acl.NextTopics;
            response.LineByLine = _tutorService.ExplainLineByLine(response.CorrectedCode, language);
            response.TutorSummary = $"ACL engine: {acl.EnginePath}";

            response.Progress = await _progressStore.RecordAsync(
                request.UserId,
                response.DetectedLanguage,
                acl.Issues);

            return await MergeGeminiAsync(request, language, response, request.Code, acl.Issues, cancellationToken);
        }

        // Default languages
        var fix = _codeFixService.Fix(request.Code, language);

        response.CorrectedCode = fix.FixedCode;
        response.Explanation = fix.Explanation;
        response.WhyWrong = fix.WhyWrong;
        response.Suggestions = fix.Issues;
        response.NextTopics = _tutorService.BuildNextTopics(language);
        response.LineByLine = _tutorService.ExplainLineByLine(response.CorrectedCode, language);
        response.TutorSummary = _tutorService.BuildLesson(language, request.Code).Summary;

        response.Progress = await _progressStore.RecordAsync(
            request.UserId,
            response.DetectedLanguage,
            fix.Issues);

        return await MergeGeminiAsync(request, language, response, request.Code, fix.Issues, cancellationToken);
    }

    // ✅ FIX: Missing method that AIController needs
    public async Task<UserProgress> GetProgressAsync(string userId)
    {
        return await _progressStore.GetAsync(userId);
    }

    private async Task<CodeResponse> MergeGeminiAsync(
        CodeRequest request,
        ProgrammingLanguage language,
        CodeResponse fallback,
        string originalCode,
        IEnumerable<string> issues,
        CancellationToken cancellationToken)
    {
        var gemini = await _geminiService.TryAnalyzeAsync(
            request,
            language,
            CreateFixResult(fallback, originalCode, issues),
            cancellationToken);

        if (gemini is null)
            return fallback;

        fallback.DetectedLanguage = string.IsNullOrWhiteSpace(gemini.DetectedLanguage) ? fallback.DetectedLanguage : gemini.DetectedLanguage;
        fallback.Explanation = string.IsNullOrWhiteSpace(gemini.Explanation) ? fallback.Explanation : gemini.Explanation;
        fallback.WhyWrong = string.IsNullOrWhiteSpace(gemini.WhyWrong) ? fallback.WhyWrong : gemini.WhyWrong;
        fallback.CorrectedCode = string.IsNullOrWhiteSpace(gemini.CorrectedCode) ? fallback.CorrectedCode : gemini.CorrectedCode;
        fallback.LineByLine = gemini.LineByLine.Count > 0 ? gemini.LineByLine : fallback.LineByLine;
        fallback.Suggestions = gemini.Suggestions.Count > 0 ? gemini.Suggestions : fallback.Suggestions;
        fallback.NextTopics = gemini.NextTopics.Count > 0 ? gemini.NextTopics : fallback.NextTopics;
        fallback.TutorSummary = string.IsNullOrWhiteSpace(gemini.TutorSummary) ? fallback.TutorSummary : gemini.TutorSummary;
        fallback.ExecutionOutput = string.IsNullOrWhiteSpace(gemini.ExecutionOutput) ? fallback.ExecutionOutput : gemini.ExecutionOutput;

        return fallback;
    }

    private static CodeFixResult CreateFixResult(
        CodeResponse response,
        string originalCode,
        IEnumerable<string> issues)
    {
        return new CodeFixResult
        {
            Language = response.DetectedLanguage,
            OriginalCode = originalCode,
            FixedCode = response.CorrectedCode,
            Explanation = response.Explanation,
            WhyWrong = response.WhyWrong,
            Issues = issues.ToList()
        };
    }
}