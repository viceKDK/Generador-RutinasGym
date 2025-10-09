using GymRoutineGenerator.Application.Common;
using GymRoutineGenerator.Application.DTOs;

namespace GymRoutineGenerator.Application.Queries.Exercises;

/// <summary>
/// Query para obtener todos los ejercicios
/// </summary>
public record GetAllExercisesQuery : IQuery<Result<List<ExerciseDto>>>;
