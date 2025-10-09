namespace GymRoutineGenerator.Domain.Common;

/// <summary>
/// Interfaz para eventos del dominio
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
