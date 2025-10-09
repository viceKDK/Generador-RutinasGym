using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.ValueObjects;

namespace GymRoutineGenerator.Domain.Repositories;

/// <summary>
/// Interfaz del repositorio de ejercicios (definida en el dominio)
/// </summary>
public interface IExerciseRepository
{
    Task<Exercise?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Exercise>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Exercise>> GetActiveExercisesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Exercise>> GetByMuscleGroupAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default);
    Task<IEnumerable<Exercise>> GetByEquipmentAsync(EquipmentType equipment, CancellationToken cancellationToken = default);
    Task<IEnumerable<Exercise>> GetByDifficultyAsync(DifficultyLevel difficulty, CancellationToken cancellationToken = default);
    Task<Exercise> AddAsync(Exercise exercise, CancellationToken cancellationToken = default);
    Task UpdateAsync(Exercise exercise, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
