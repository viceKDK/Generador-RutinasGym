using GymRoutineGenerator.Application.Common;
using GymRoutineGenerator.Domain.Aggregates;
using MediatR;

namespace GymRoutineGenerator.Application.Commands.WorkoutPlans;

/// <summary>
/// Command to generate a complete workout plan with exercises using AI/algorithm
/// </summary>
public record GenerateWorkoutPlanCommand : IRequest<Result<GenerateWorkoutPlanResult>>
{
    public string UserName { get; init; } = string.Empty;
    public int Age { get; init; }
    public string Gender { get; init; } = string.Empty;
    public string FitnessLevel { get; init; } = string.Empty;
    public int TrainingDays { get; init; }
    public List<string> Goals { get; init; } = new();
}

/// <summary>
/// Result containing the generated routine text
/// Returns a simple text representation for now
/// TODO: Return structured WorkoutPlanDto when fully migrated to Domain models
/// </summary>
public record GenerateWorkoutPlanResult
{
    public string RoutineText { get; init; } = string.Empty;
    public WorkoutPlan? WorkoutPlan { get; init; }
}
