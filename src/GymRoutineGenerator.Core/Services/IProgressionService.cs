using GymRoutineGenerator.Core.Models;

namespace GymRoutineGenerator.Core.Services
{
    public interface IProgressionService
    {
        Task<ProgressionAnalysis> AnalyzeUserProgressionAsync(int userId);
        Task<ProgressionRecommendation> GetPersonalizedRecommendationsAsync(int userId);
        Task<List<ProgressMetric>> GetProgressMetricsAsync(int userId, TimeRange timeRange);
        Task<bool> ShouldIncreaseIntensityAsync(int userId, int exerciseId);
        Task<Dictionary<string, object>> GenerateProgressReportAsync(int userId);
    }
}