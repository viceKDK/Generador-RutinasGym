using GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Data.Management;

public interface IExerciseManagementService
{
    // CRUD Operations
    Task<ExerciseManagementResult> CreateExerciseAsync(ExerciseCreateRequest request);
    Task<ExerciseManagementResult> UpdateExerciseAsync(ExerciseUpdateRequest request);
    Task<ExerciseManagementResult> DeleteExerciseAsync(int exerciseId, bool forceDelete = false);
    Task<Exercise?> GetExerciseByIdAsync(int exerciseId);
    Task<List<Exercise>> GetAllExercisesAsync(bool includeInactive = false);

    // Validation
    Task<ExerciseValidationResult> ValidateExerciseAsync(ExerciseCreateRequest request, int? excludeExerciseId = null);
    Task<ExerciseDeletionCheck> CheckDeletionAsync(int exerciseId);

    // Bulk Operations
    Task<BulkOperationResult> ExecuteBulkOperationAsync(BulkExerciseOperation operation);
    Task<ExerciseManagementResult> DuplicateExerciseAsync(int exerciseId, string newName, string newSpanishName);
    Task<BulkOperationResult> ImportExercisesAsync(List<ExerciseCreateRequest> exercises);

    // Image Management
    Task<ExerciseManagementResult> AddExerciseImageAsync(int exerciseId, ExerciseImageUpload image);
    Task<ExerciseManagementResult> UpdateExerciseImageAsync(int imageId, ExerciseImageUpload image);
    Task<ExerciseManagementResult> DeleteExerciseImageAsync(int imageId);
    Task<List<ExerciseImage>> GetExerciseImagesAsync(int exerciseId);

    // Relationship Management
    Task<ExerciseManagementResult> SetParentExerciseAsync(int exerciseId, int? parentExerciseId);
    Task<ExerciseManagementResult> AddSecondaryMuscleAsync(int exerciseId, int muscleGroupId);
    Task<ExerciseManagementResult> RemoveSecondaryMuscleAsync(int exerciseId, int muscleGroupId);

    // Statistics and Reporting
    Task<ExerciseManagementSummary> GetManagementSummaryAsync();
    Task<List<Exercise>> GetExercisesNeedingImagesAsync();
    Task<List<Exercise>> GetExercisesWithIssuesAsync();
    Task<List<Exercise>> GetRecentlyModifiedExercisesAsync(int days = 7);

    // Lookup Data
    Task<List<MuscleGroup>> GetMuscleGroupsAsync();
    Task<List<EquipmentType>> GetEquipmentTypesAsync();
}