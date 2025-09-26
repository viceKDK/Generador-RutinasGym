using GymRoutineGenerator.Core.Enums;

namespace GymRoutineGenerator.Data.Search;

public class ExerciseSearchCriteria
{
    public string? SearchTerm { get; set; }
    public List<int>? MuscleGroupIds { get; set; }
    public List<int>? EquipmentTypeIds { get; set; }
    public List<DifficultyLevel>? DifficultyLevels { get; set; }
    public List<ExerciseType>? ExerciseTypes { get; set; }
    public bool? IsActive { get; set; } = true;
    public int? DurationMinSeconds { get; set; }
    public int? DurationMaxSeconds { get; set; }
    public bool IncludeSecondaryMuscles { get; set; } = true;
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 50;
    public ExerciseSearchSort SortBy { get; set; } = ExerciseSearchSort.Name;
    public bool SortDescending { get; set; } = false;
}

public class ExerciseSearchResult
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SpanishName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DifficultyLevel DifficultyLevel { get; set; }
    public ExerciseType ExerciseType { get; set; }
    public int? DurationSeconds { get; set; }
    public bool IsActive { get; set; }

    // Muscle and Equipment Info
    public MuscleGroupInfo PrimaryMuscleGroup { get; set; } = new();
    public EquipmentTypeInfo EquipmentType { get; set; } = new();
    public List<MuscleGroupInfo> SecondaryMuscleGroups { get; set; } = new();

    // Image Info
    public List<ExerciseImageInfo> Images { get; set; } = new();
    public string? PrimaryImagePath { get; set; }

    // Search Relevance
    public double SearchScore { get; set; }
    public List<string> MatchedTerms { get; set; } = new();
}

public class MuscleGroupInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SpanishName { get; set; } = string.Empty;
}

public class EquipmentTypeInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SpanishName { get; set; } = string.Empty;
    public bool RequiresGym { get; set; }
}

public class ExerciseImageInfo
{
    public int Id { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string ImagePosition { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class SearchResultsPage
{
    public List<ExerciseSearchResult> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
    public SearchStatistics Statistics { get; set; } = new();
    public List<SearchSuggestion> Suggestions { get; set; } = new();
}

public class SearchStatistics
{
    public TimeSpan SearchDuration { get; set; }
    public Dictionary<string, int> ResultsByMuscleGroup { get; set; } = new();
    public Dictionary<string, int> ResultsByEquipment { get; set; } = new();
    public Dictionary<DifficultyLevel, int> ResultsByDifficulty { get; set; } = new();
    public Dictionary<ExerciseType, int> ResultsByType { get; set; } = new();
}

public class SearchSuggestion
{
    public string Text { get; set; } = string.Empty;
    public SearchSuggestionType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public ExerciseSearchCriteria? SuggestedCriteria { get; set; }
}

public enum ExerciseSearchSort
{
    Name = 0,
    SpanishName = 1,
    Difficulty = 2,
    ExerciseType = 3,
    MuscleGroup = 4,
    Equipment = 5,
    Duration = 6,
    Relevance = 7,
    CreatedDate = 8,
    UpdatedDate = 9
}

public enum SearchSuggestionType
{
    SimilarTerm = 0,
    AlternativeFilter = 1,
    PopularExercise = 2,
    RelatedMuscleGroup = 3,
    AlternativeEquipment = 4,
    DifficultyAdjustment = 5
}