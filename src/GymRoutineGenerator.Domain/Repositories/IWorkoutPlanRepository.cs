using GymRoutineGenerator.Domain.Aggregates;

namespace GymRoutineGenerator.Domain.Repositories;

/// <summary>
/// Interfaz del repositorio de planes de entrenamiento (definida en el dominio)
/// </summary>
public interface IWorkoutPlanRepository
{
    Task<WorkoutPlan?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutPlan>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutPlan>> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<WorkoutPlan?> GetLatestByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<WorkoutPlan> AddAsync(WorkoutPlan plan, CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkoutPlan plan, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
