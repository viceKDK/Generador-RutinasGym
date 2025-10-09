using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.Common;

namespace GymRoutineGenerator.Domain.Events;

/// <summary>
/// Evento de dominio: Ejercicio creado
/// </summary>
public class ExerciseCreatedEvent : IDomainEvent
{
    public Exercise Exercise { get; }
    public DateTime OccurredOn { get; }

    public ExerciseCreatedEvent(Exercise exercise)
    {
        Exercise = exercise;
        OccurredOn = DateTime.UtcNow;
    }
}
