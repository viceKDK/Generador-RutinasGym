using GymRoutineGenerator.Application.Common;
using GymRoutineGenerator.Application.DTOs;

namespace GymRoutineGenerator.Application.Commands.WorkoutPlans;

/// <summary>
/// Command para crear un plan de entrenamiento
/// </summary>
public record CreateWorkoutPlanCommand(
    string Name,
    string UserName,
    int UserAge,
    string Gender,
    string UserLevel,
    int TrainingDaysPerWeek,
    string? Description = null,
    List<string>? UserLimitations = null
) : ICommand<WorkoutPlanDto>;
