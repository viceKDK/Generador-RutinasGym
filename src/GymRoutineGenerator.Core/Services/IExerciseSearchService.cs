using GymRoutineGenerator.Core.Models;
using DifficultyLevel = GymRoutineGenerator.Core.Enums.DifficultyLevel;

namespace GymRoutineGenerator.Core.Services
{
    public interface IExerciseSearchService
    {
        Task<SearchResult> SearchExercisesAsync(SearchQuery query);
        Task<List<Exercise>> SearchByNaturalLanguageAsync(string description);
        Task<Exercise?> IdentifyExerciseFromImageAsync(byte[] imageData);
        Task<List<Exercise>> GetSimilarExercisesAsync(int exerciseId);
        Task<SearchResult> SearchInDocsDirectoryAsync(string pattern);
        Task<List<Exercise>> GetTrendingExercisesAsync();
        Task<List<Exercise>> GetRecommendedExercisesAsync(UserProfile profile);
        Task<SearchResult> AdvancedSearchAsync(AdvancedSearchQuery query);
    }

    public class SearchQuery
    {
        public string TextQuery { get; set; } = string.Empty;
        public List<string> MuscleGroups { get; set; } = new();
        public List<string> EquipmentTypes { get; set; } = new();
        public DifficultyLevel? MinDifficulty { get; set; }
        public DifficultyLevel? MaxDifficulty { get; set; }
        public List<MovementType> MovementTypes { get; set; } = new();
        public int? MaxDurationSeconds { get; set; }
        public List<DataSource> Sources { get; set; } = new();
        public bool IncludeVideos { get; set; }
        public List<string> ExcludeExercises { get; set; } = new();
        public UserProfile? UserProfile { get; set; }
        public int MaxResults { get; set; } = 20;
        public SearchSort SortBy { get; set; } = SearchSort.Relevance;
    }

    public class AdvancedSearchQuery : SearchQuery
    {
        public List<string> Tags { get; set; } = new();
        public bool RequireImages { get; set; }
        public bool RequireInstructions { get; set; }
        public DateRange? CreatedDateRange { get; set; }
        public int? MinRating { get; set; }
        public List<string> AuthorFilter { get; set; } = new();
        public bool FavoriteOnly { get; set; }
    }

    public class SearchResult
    {
        public List<ExerciseSearchResult> Exercises { get; set; } = new();
        public int TotalCount { get; set; }
        public SearchMetadata Metadata { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
        public string QueryInterpretation { get; set; } = string.Empty;
        public TimeSpan SearchDuration { get; set; }
    }

    public class ExerciseSearchResult
    {
        public Exercise Exercise { get; set; } = new();
        public float RelevanceScore { get; set; }
        public List<string> MatchReasons { get; set; } = new();
        public string Snippet { get; set; } = string.Empty;
        public List<string> HighlightedTerms { get; set; } = new();
        public DataSource Source { get; set; }
    }

    public class SearchMetadata
    {
        public Dictionary<string, int> MuscleGroupBreakdown { get; set; } = new();
        public Dictionary<string, int> DifficultyBreakdown { get; set; } = new();
        public Dictionary<string, int> EquipmentBreakdown { get; set; } = new();
        public Dictionary<DataSource, int> SourceBreakdown { get; set; } = new();
        public List<string> PopularTags { get; set; } = new();
    }

    public class ImageRecognitionResult
    {
        public string ExerciseName { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public List<string> IdentifiedMuscleGroups { get; set; } = new();
        public List<string> IdentifiedEquipment { get; set; } = new();
        public MovementType EstimatedMovementType { get; set; }
        public List<Exercise> SimilarExercises { get; set; } = new();
        public string ExplanationMessage { get; set; } = string.Empty;
        public List<string> AlternativeInterpretations { get; set; } = new();
        public ImageQuality? Quality { get; set; }
    }

    public enum MovementType
    {
        Push,
        Pull,
        Squat,
        Hinge,
        Carry,
        Rotation,
        Isometric,
        Explosive,
        Endurance
    }

    public enum DataSource
    {
        Database,
        DocsDirectory,
        UserFavorites,
        AIGenerated,
        ExternalAPI,
        UserCreated
    }

    public enum SearchSort
    {
        Relevance,
        Popularity,
        Difficulty,
        Name,
        CreatedDate,
        Rating,
        Duration
    }

    public class DateRange
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}