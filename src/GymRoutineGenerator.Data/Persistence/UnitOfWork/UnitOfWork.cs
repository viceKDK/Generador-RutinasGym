using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Persistence.Repositories;
using GymRoutineGenerator.Domain.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace GymRoutineGenerator.Data.Persistence.UnitOfWork;

/// <summary>
/// Implementación del patrón Unit of Work usando EF Core
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly GymRoutineContext _context;
    private IDbContextTransaction? _transaction;

    private IExerciseRepository? _exerciseRepository;
    private IWorkoutPlanRepository? _workoutPlanRepository;

    public UnitOfWork(GymRoutineContext context)
    {
        _context = context;
    }

    public IExerciseRepository Exercises
    {
        get
        {
            _exerciseRepository ??= new DomainExerciseRepository(_context);
            return _exerciseRepository;
        }
    }

    public IWorkoutPlanRepository WorkoutPlans
    {
        get
        {
            _workoutPlanRepository ??= new DomainWorkoutPlanRepository(_context);
            return _workoutPlanRepository;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("No hay transacción activa");

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
