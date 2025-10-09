using System;
using System.Collections.Generic;

namespace GymRoutineGenerator.Infrastructure.Exercises;

public interface IDocumentExerciseCatalog
{
    DocumentExerciseRecord? FindByName(string name);
    IReadOnlyCollection<DocumentExerciseRecord> GetAll();
}

public sealed class DocumentExerciseRecord
{
    public string SpanishName { get; init; } = string.Empty;
    public string EnglishName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Equipment { get; init; } = string.Empty;
    public IReadOnlyList<string> MuscleGroups { get; init; } = Array.Empty<string>();
    public string? ImagePath { get; init; }
}
