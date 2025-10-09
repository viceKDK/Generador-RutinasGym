using GymRoutineGenerator.Domain.Common;
using GymRoutineGenerator.Domain.ValueObjects;

namespace GymRoutineGenerator.Domain.Aggregates;

/// <summary>
/// Agregado raíz para Rutina (un día de entrenamiento)
/// </summary>
public class Routine : Entity
{
    private readonly List<RoutineExercise> _exercises = new();

    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int DayNumber { get; private set; }  // Día de la semana (1-7)
    public IReadOnlyCollection<RoutineExercise> Exercises => _exercises.AsReadOnly();

    private Routine()
    {
        Name = string.Empty;
    }

    private Routine(string name, int dayNumber, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la rutina no puede estar vacío", nameof(name));

        if (dayNumber < 1 || dayNumber > 7)
            throw new ArgumentException("El día debe estar entre 1 y 7", nameof(dayNumber));

        Name = name;
        DayNumber = dayNumber;
        Description = description;
    }

    public static Routine Create(string name, int dayNumber, string? description = null)
    {
        return new Routine(name, dayNumber, description);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la rutina no puede estar vacío", nameof(name));

        Name = name;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }

    public void AddExercise(Exercise exercise, int order, List<ExerciseSet> sets, string? notes = null)
    {
        if (exercise == null)
            throw new ArgumentNullException(nameof(exercise));

        if (sets == null || sets.Count == 0)
            throw new ArgumentException("Debe haber al menos una serie", nameof(sets));

        var routineExercise = new RoutineExercise(exercise, order, sets, notes);
        _exercises.Add(routineExercise);

        // Reordenar ejercicios por orden
        _exercises.Sort((a, b) => a.Order.CompareTo(b.Order));
    }

    public void RemoveExercise(int exerciseId)
    {
        var exercise = _exercises.FirstOrDefault(e => e.Exercise.Id == exerciseId);
        if (exercise != null)
        {
            _exercises.Remove(exercise);
        }
    }

    public void ReorderExercise(int exerciseId, int newOrder)
    {
        var exercise = _exercises.FirstOrDefault(e => e.Exercise.Id == exerciseId);
        if (exercise != null)
        {
            exercise.UpdateOrder(newOrder);
            _exercises.Sort((a, b) => a.Order.CompareTo(b.Order));
        }
    }

    public int GetTotalExercises() => _exercises.Count;

    public int GetTotalSets() => _exercises.Sum(e => e.Sets.Count);

    public IEnumerable<MuscleGroup> GetTargetedMuscleGroups()
    {
        return _exercises
            .SelectMany(e => e.Exercise.TargetMuscles)
            .Distinct();
    }
}

/// <summary>
/// Entidad que representa un ejercicio dentro de una rutina
/// </summary>
public class RoutineExercise : Entity
{
    private readonly List<ExerciseSet> _sets = new();

    public Exercise Exercise { get; private set; }
    public int Order { get; private set; }
    public IReadOnlyCollection<ExerciseSet> Sets => _sets.AsReadOnly();
    public string? Notes { get; private set; }

    private RoutineExercise()
    {
        Exercise = null!;
    }

    internal RoutineExercise(Exercise exercise, int order, List<ExerciseSet> sets, string? notes = null)
    {
        Exercise = exercise ?? throw new ArgumentNullException(nameof(exercise));
        Order = order;
        _sets.AddRange(sets);
        Notes = notes;
    }

    internal void UpdateOrder(int order)
    {
        if (order < 0)
            throw new ArgumentException("El orden no puede ser negativo", nameof(order));

        Order = order;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
    }

    public void AddSet(ExerciseSet set)
    {
        if (set == null)
            throw new ArgumentNullException(nameof(set));

        _sets.Add(set);
    }

    public void RemoveSet(int index)
    {
        if (index >= 0 && index < _sets.Count)
        {
            _sets.RemoveAt(index);
        }
    }
}
