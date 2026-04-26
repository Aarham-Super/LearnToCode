using System.Text.Json;
using LearnToCode.Models;

namespace LearnToCode.Services;

public sealed class LearningProgressStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly Dictionary<string, UserProgress> _cache = new(StringComparer.OrdinalIgnoreCase);

    public LearningProgressStore(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configured = configuration["Learning:ProgressFile"] ?? "App_Data/user-progress.json";
        _filePath = Path.GetFullPath(Path.Combine(environment.ContentRootPath, configured));
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        LoadFromDisk();
    }

    public async Task<UserProgress> RecordAsync(string userId, string language, IEnumerable<string> issues)
    {
        await _gate.WaitAsync();
        try
        {
            if (!_cache.TryGetValue(userId, out var progress))
            {
                progress = new UserProgress { UserId = userId };
                _cache[userId] = progress;
            }

            progress.TotalSubmissions++;
            progress.LastLanguage = language;
            progress.LastUpdatedUtc = DateTimeOffset.UtcNow;

            if (!string.IsNullOrWhiteSpace(language) && !progress.LearnedTopics.Contains(language, StringComparer.OrdinalIgnoreCase))
            {
                progress.LearnedTopics.Add(language);
            }

            foreach (var issue in issues.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                progress.RecentMistakes.Insert(0, issue);
                if (progress.RecentMistakes.Count > 8)
                {
                    progress.RecentMistakes.RemoveAt(progress.RecentMistakes.Count - 1);
                }

                progress.MistakeCounts.TryGetValue(issue, out var count);
                progress.MistakeCounts[issue] = count + 1;
            }

            if (issues.Any())
            {
                progress.FixedSubmissions++;
            }

            await SaveAsync();
            return Clone(progress);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<UserProgress> GetAsync(string userId)
    {
        await _gate.WaitAsync();
        try
        {
            if (_cache.TryGetValue(userId, out var progress))
            {
                return Clone(progress);
            }

            var created = new UserProgress { UserId = userId };
            _cache[userId] = created;
            await SaveAsync();
            return Clone(created);
        }
        finally
        {
            _gate.Release();
        }
    }

    private void LoadFromDisk()
    {
        if (!File.Exists(_filePath))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            var items = JsonSerializer.Deserialize<List<UserProgress>>(json, JsonOptions) ?? [];
            foreach (var item in items)
            {
                _cache[item.UserId] = item;
            }
        }
        catch
        {
            _cache.Clear();
        }
    }

    private async Task SaveAsync()
    {
        var items = _cache.Values.OrderBy(x => x.UserId).ToList();
        var json = JsonSerializer.Serialize(items, JsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }

    private static UserProgress Clone(UserProgress source)
    {
        return new UserProgress
        {
            UserId = source.UserId,
            TotalSubmissions = source.TotalSubmissions,
            FixedSubmissions = source.FixedSubmissions,
            LastLanguage = source.LastLanguage,
            LastUpdatedUtc = source.LastUpdatedUtc,
            MistakeCounts = new Dictionary<string, int>(source.MistakeCounts, StringComparer.OrdinalIgnoreCase),
            RecentMistakes = [.. source.RecentMistakes],
            LearnedTopics = [.. source.LearnedTopics]
        };
    }
}
