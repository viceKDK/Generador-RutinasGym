using GymRoutineGenerator.Domain.Repositories;
using GymRoutineGenerator.Data.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace GymRoutineGenerator.Infrastructure.Persistence;

/// <summary>
/// Implementación del patrón Unit of Work usando EF Core
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly GymRoutineContext _context;
    private IDbContextTransaction? _currentTransaction;
    private IExerciseRepository? _exerciseRepository;
    private IWorkoutPlanRepository? _workoutPlanRepository;

    public UnitOfWork(GymRoutineContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
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
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction in progress to commit.");
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction in progress to rollback.");
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
