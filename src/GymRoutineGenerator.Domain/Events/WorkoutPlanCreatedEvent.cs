using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.Common;

namespace GymRoutineGenerator.Domain.Events;

/// <summary>
/// Evento de dominio: Plan de entrenamiento creado
/// </summary>
public class WorkoutPlanCreatedEvent : IDomainEvent
{
    public WorkoutPlan WorkoutPlan { get; }
    public DateTime OccurredOn { get; }

    public WorkoutPlanCreatedEvent(WorkoutPlan workoutPlan)
    {
        WorkoutPlan = workoutPlan;
        OccurredOn = DateTime.UtcNow;
    }
}
