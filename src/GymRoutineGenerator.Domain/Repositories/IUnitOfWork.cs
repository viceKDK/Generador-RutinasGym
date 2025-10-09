namespace GymRoutineGenerator.Domain.Repositories;

/// <summary>
/// Interfaz para el patrón Unit of Work (definida en el dominio)
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IExerciseRepository Exercises { get; }
    IWorkoutPlanRepository WorkoutPlans { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
