using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.Repositories;

namespace GymRoutineGenerator.Infrastructure.Persistence;

/// <summary>
/// Repositorio de WorkoutPlan que delega la persistencia a la implementación de la capa Data.
/// Esto mantiene la separación entre Infrastructure y Data sin duplicar lógica de mapeo.
/// </summary>
public class DomainWorkoutPlanRepository : IWorkoutPlanRepository
{
    private readonly GymRoutineGenerator.Data.Persistence.Repositories.DomainWorkoutPlanRepository _inner;

    public DomainWorkoutPlanRepository(GymRoutineContext context)
    {
        _inner = new GymRoutineGenerator.Data.Persistence.Repositories.DomainWorkoutPlanRepository(context);
    }

    public Task<WorkoutPlan?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _inner.GetByIdAsync(id, cancellationToken);

    public Task<IEnumerable<WorkoutPlan>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _inner.GetAllAsync(cancellationToken);

    public Task<IEnumerable<WorkoutPlan>> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default) =>
        _inner.GetByUserNameAsync(userName, cancellationToken);

    public Task<WorkoutPlan?> GetLatestByUserNameAsync(string userName, CancellationToken cancellationToken = default) =>
        _inner.GetLatestByUserNameAsync(userName, cancellationToken);

    public Task<WorkoutPlan> AddAsync(WorkoutPlan plan, CancellationToken cancellationToken = default) =>
        _inner.AddAsync(plan, cancellationToken);

    public Task UpdateAsync(WorkoutPlan plan, CancellationToken cancellationToken = default) =>
        _inner.UpdateAsync(plan, cancellationToken);

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default) =>
        _inner.DeleteAsync(id, cancellationToken);
}
