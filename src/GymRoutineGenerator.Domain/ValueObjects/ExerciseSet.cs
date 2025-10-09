using GymRoutineGenerator.Domain.Common;

namespace GymRoutineGenerator.Domain.ValueObjects;

/// <summary>
/// Value Object que representa una serie de un ejercicio
/// </summary>
public sealed class ExerciseSet : ValueObject
{
    public int Repetitions { get; private set; }
    public int? Weight { get; private set; }  // Peso en kg (opcional)
    public int RestSeconds { get; private set; }
    public string? Notes { get; private set; }

    private ExerciseSet(int repetitions, int? weight, int restSeconds, string? notes)
    {
        Repetitions = repetitions;
        Weight = weight;
        RestSeconds = restSeconds;
        Notes = notes;
    }

    public static ExerciseSet Create(int repetitions, int? weight = null, int restSeconds = 60, string? notes = null)
    {
        if (repetitions <= 0)
            throw new ArgumentException("Las repeticiones deben ser mayores a 0", nameof(repetitions));

        if (weight.HasValue && weight.Value < 0)
            throw new ArgumentException("El peso no puede ser negativo", nameof(weight));

        if (restSeconds < 0)
            throw new ArgumentException("El descanso no puede ser negativo", nameof(restSeconds));

        return new ExerciseSet(repetitions, weight, restSeconds, notes);
    }

    public ExerciseSet WithWeight(int weight)
    {
        return Create(Repetitions, weight, RestSeconds, Notes);
    }

    public ExerciseSet WithRepetitions(int repetitions)
    {
        return Create(repetitions, Weight, RestSeconds, Notes);
    }

    public ExerciseSet WithRest(int restSeconds)
    {
        return Create(Repetitions, Weight, restSeconds, Notes);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Repetitions;
        yield return Weight;
        yield return RestSeconds;
    }

    public override string ToString()
    {
        var weightStr = Weight.HasValue ? $"{Weight}kg x " : "";
        return $"{weightStr}{Repetitions} reps - {RestSeconds}s descanso";
    }
}
