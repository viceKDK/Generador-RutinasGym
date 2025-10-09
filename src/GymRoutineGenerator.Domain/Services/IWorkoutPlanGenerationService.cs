using GymRoutineGenerator.Domain.Aggregates;

namespace GymRoutineGenerator.Domain.Services;

/// <summary>
/// Domain service for workout plan generation
/// </summary>
public interface IWorkoutPlanGenerationService
{
    /// <summary>
    /// Generates a personalized workout plan for a user
    /// </summary>
    Task<WorkoutPlanGenerationResult> GenerateWorkoutPlanAsync(
        string userName,
        int age,
        string gender,
        string fitnessLevel,
        int trainingDays,
        List<string> goals,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an alternative workout routine text
    /// </summary>
    Task<string> GenerateAlternativeRoutineTextAsync(
        string userName,
        int age,
        string gender,
        string fitnessLevel,
        int trainingDays,
        List<string> goals,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if AI-based generation is available
    /// </summary>
    Task<bool> IsAIAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current status of the AI service
    /// </summary>
    Task<string> GetAIStatusAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Result returned by workout plan generation containing the aggregate and narrative text
/// </summary>
public record WorkoutPlanGenerationResult(WorkoutPlan WorkoutPlan, string RoutineText);
