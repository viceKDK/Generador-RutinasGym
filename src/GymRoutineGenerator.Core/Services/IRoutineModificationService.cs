using GymRoutineGenerator.Core.Models;

namespace GymRoutineGenerator.Core.Services
{
    public interface IRoutineModificationService
    {
        Task<UserRoutine> ApplyModificationAsync(int routineId, ExerciseModification modification);
        Task<List<ExerciseAlternative>> GetAlternativeExercisesAsync(int exerciseId, UserProfile profile);
        Task<UserRoutine> CreateVariationAsync(int routineId, string variationType);
        Task<UserRoutine> AdaptForLimitationsAsync(int routineId, List<UserPhysicalLimitation> limitations);
        Task<ExerciseModification> SuggestModificationAsync(int exerciseId, UserProfile profile);
        Task<bool> ValidateModificationAsync(ExerciseModification modification);
    }
}