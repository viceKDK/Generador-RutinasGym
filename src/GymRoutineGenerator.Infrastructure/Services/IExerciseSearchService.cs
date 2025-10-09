using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Core.Models;
using GymRoutineGenerator.Core.Services;

namespace GymRoutineGenerator.Infrastructure.Services
{
    // This interface extends the core IExerciseSearchService with infrastructure-specific features
    public interface IInfrastructureExerciseSearchService : IExerciseSearchService
    {
        // Infrastructure-specific methods
        Task<List<ExerciseFileInfo>> SearchExerciseFilesAsync(string query);
        Task<List<ExerciseFileInfo>> GetExerciseFilesInDirectoryAsync(string directory);
        Task SaveSearchHistoryAsync(ExerciseSearchHistory searchHistory);
        Task<List<ExerciseSearchHistory>> GetUserSearchHistoryAsync(string userId, int limit = 20);
        Task<Dictionary<string, int>> GetSearchStatsAsync(string userId);
        Task<List<string>> GetPopularSearchTermsAsync(int limit = 10);
    }

    public class ExerciseSearchHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string SearchQuery { get; set; } = string.Empty;
        public DateTime SearchDate { get; set; }
        public int ResultsCount { get; set; }
        public string SearchType { get; set; } = string.Empty;
    }


    public class ExerciseFileInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty; // "Image", "Video", "Document"
        public string ExerciseName { get; set; } = string.Empty;
        public string MuscleGroup { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }
    }
}