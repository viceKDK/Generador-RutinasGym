using GymRoutineGenerator.Core.Models;

namespace GymRoutineGenerator.Core.Services
{
    public interface ISafetyValidationService
    {
        Task<SafetyValidationResult> ValidateExerciseForUserAsync(Exercise exercise, UserProfile user);
        Task<SafetyValidationResult> ValidateRoutineModificationAsync(ExerciseModification modification, UserProfile user);
        Task<SafetyValidationResult> ValidateCompleteRoutineAsync(UserRoutine routine, UserProfile user);
        Task<List<SafetyWarning>> GetSafetyWarningsAsync(Exercise exercise, UserProfile user);
        Task<List<string>> GetSafetyRecommendationsAsync(UserProfile user);
        Task<bool> IsExerciseSafeForUserAsync(int exerciseId, int userId);
    }
}