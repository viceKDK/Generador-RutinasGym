using GymRoutineGenerator.Application.Common;
using GymRoutineGenerator.Application.DTOs;

namespace GymRoutineGenerator.Application.Queries.Exercises;

/// <summary>
/// Query para obtener el catálogo resumido de ejercicios para la UI.
/// </summary>
public record GetExerciseCatalogQuery : IQuery<Result<List<ExerciseCatalogItemDto>>>;
