using GymRoutineGenerator.Application.Common;
using GymRoutineGenerator.Application.DTOs;

namespace GymRoutineGenerator.Application.Queries.WorkoutPlans;

/// <summary>
/// Query para obtener un plan de entrenamiento por ID
/// </summary>
public record GetWorkoutPlanByIdQuery(int Id) : IQuery<Result<WorkoutPlanDto>>;
