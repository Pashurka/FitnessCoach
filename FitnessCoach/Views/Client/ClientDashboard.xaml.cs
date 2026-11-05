

    /// <summary>
    /// Логування процесу категорізації
    /// </summary>
    private void LogCategorization(
        WorkoutSession session,
        string normalizedText,
        WorkoutCategory finalCategory,
        KeywordAnalysisResult keywordResult,
        WorkoutCategory mlResult)
    {
        _logger.LogInformation(
            "Категорізація тренування {SessionId}: " +
            "Текст: '{Text}', " +
            "Keyword: {KeywordCat} ({KeywordConfidence:P0}), " +
            "ML: {MLCat}, " +
            "Результат: {FinalCat}",
            session.SessionId,
            normalizedText.Length > 50 ? normalizedText.Substring(0, 50) + "..." : normalizedText,
            keywordResult.Category,
            keywordResult.Confidence,
            mlResult,
            finalCategory);
    }

    /// <summary>
    /// Статистика категорій для клієнта
    /// </summary>
    public async Task<TrainingTypeStats> GetCategoryStatisticsAsync(Guid clientId, DateTimeRange range)
    {
        // Отримання всіх тренувань клієнта за період
        var sessions = await GetClientSessionsAsync(clientId, range);

        // Категорізація всіх тренувань
        var categories = await CategorizeBatchAsync(sessions);

        // Підрахунок статистики
        var stats = new TrainingTypeStats
        {
            ClientId = clientId,
            Period = range,
            TotalSessions = sessions.Count,
            CategoryDistribution = categories
                .GroupBy(c => c)
                .ToDictionary(g => g.Key, g => new CategoryStats
                {
                    Count = g.Count(),
                    Percentage = (double)g.Count() / sessions.Count * 100
                }),
            MostCommonCategory = categories
                .GroupBy(c => c)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? WorkoutCategory.Unknown
        };

        // Аналіз тенденцій
        stats.Trend = AnalyzeTrend(stats, await GetPreviousPeriodStatsAsync(clientId, range));

        return stats;
    }

    private async Task<List<WorkoutSession>> GetClientSessionsAsync(Guid clientId, DateTimeRange range)
    {
        // Реалізація отримання тренувань з репозиторію
        // Повертаємо заглушку для прикладу
        return new List<WorkoutSession>();
    }

    private async Task<TrainingTypeStats> GetPreviousPeriodStatsAsync(Guid clientId, DateTimeRange range)
    {
        // Отримання статистики за попередній період
        return new TrainingTypeStats();
    }

    private TrendType AnalyzeTrend(TrainingTypeStats current, TrainingTypeStats previous)
    {
        // Аналіз тенденцій змін у категоріях
        return TrendType.Stable;
    }
}

// Моделі даних
public enum WorkoutCategory
{
    Unknown = 0,
    Strength = 1,      // Силове тренування
    Cardio = 2,        // Кардіо тренування
    Flexibility = 3,   // Розтяжка/гнучкість
    Mixed = 4,         // Змішане тренування
    Recovery = 5       // Відновлення
}

public enum TrendType
{
    Improving = 1,
    Declining = 2,
    Stable = 3,
    Volatile = 4
}

public class KeywordAnalysisResult
{
    public WorkoutCategory Category { get; set; }
    public int Score { get; set; }
    public double Confidence { get; set; }
    public Dictionary<WorkoutCategory, List<string>> FoundKeywords { get; set; } = new();
}

public class TrainingTypeStats
{
    public Guid ClientId { get; set; }
    public DateTimeRange Period { get; set; } = new();
    public int TotalSessions { get; set; }
    public Dictionary<WorkoutCategory, CategoryStats> CategoryDistribution { get; set; } = new();
    public WorkoutCategory MostCommonCategory { get; set; }
    public TrendType Trend { get; set; }
}

public class CategoryStats
{
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class DateTimeRange
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}

// Інтерфейси для розширень
public interface IKeywordDictionaryService
{
    Task UpdateDictionaryAsync(WorkoutCategory category, List<string> keywords);
    Task<Dictionary<WorkoutCategory, List<string>>> GetDictionaryAsync();
}

public interface IMLCategorizerService
{
    bool IsAvailable { get; }
    Task<WorkoutCategory> PredictCategoryAsync(string text);
    Task TrainModelAsync(List<TrainingExample> examples);
}

public class TrainingExample
{
    public string Text { get; set; } = string.Empty;
    public WorkoutCategory Category { get; set; }
}

// Приклад використання
public class WorkoutSession
{
    public Guid SessionId { get; set; }
    public DateTime DateTime { get; set; }
    public int DurationMinutes { get; set; }
    public string? Notes { get; set; }
    public List<Exercise> Exercises { get; set; } = new();
    public WorkoutCategory? AutoCategory { get; set; }
}

public class Exercise
{
    public ExerciseType Type { get; set; }
}

public enum ExerciseType
{
    Strength,
    Cardio,
    Flexibility
}
