using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GymRoutineGenerator.Infrastructure.Exercises;

public interface IExerciseDataProvider
{
    Task<IReadOnlyCollection<ExerciseDataSnapshot>> GetAllExercisesAsync(CancellationToken cancellationToken = default);
}

public sealed class ExerciseDataSnapshot
{
    public string EnglishName { get; init; } = string.Empty;
    public string SpanishName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Instructions { get; init; } = string.Empty;
    public string Equipment { get; init; } = string.Empty;
    public IReadOnlyCollection<string> MuscleGroups { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> SecondaryMuscles { get; init; } = Array.Empty<string>();
    public string? ImagePath { get; init; }
    public byte[]? ImageData { get; init; }
    public bool IsActive { get; init; } = true;
}
