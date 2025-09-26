using GymRoutineGenerator.Core.Enums;

namespace GymRoutineGenerator.Data.Search;

public interface IExerciseSearchService
{
    Task<SearchResultsPage> SearchExercisesAsync(ExerciseSearchCriteria criteria);
    Task<SearchResultsPage> SearchExercisesByTextAsync(string searchTerm, int skip = 0, int take = 50);
    Task<List<ExerciseSearchResult>> GetSimilarExercisesAsync(int exerciseId, int maxResults = 10);
    Task<List<ExerciseSearchResult>> GetRandomExercisesAsync(int count = 10, ExerciseSearchCriteria? filters = null);
    Task<List<string>> GetSearchSuggestionsAsync(string partialTerm, int maxSuggestions = 10);
    Task<SearchStatistics> GetSearchStatisticsAsync(ExerciseSearchCriteria criteria);
    Task<List<ExerciseSearchResult>> GetExercisesByMuscleGroupAsync(int muscleGroupId, int maxResults = 20);
    Task<List<ExerciseSearchResult>> GetExercisesByEquipmentAsync(int equipmentTypeId, int maxResults = 20);
    Task<List<ExerciseSearchResult>> GetExercisesByDifficultyAsync(DifficultyLevel difficulty, int maxResults = 20);
}