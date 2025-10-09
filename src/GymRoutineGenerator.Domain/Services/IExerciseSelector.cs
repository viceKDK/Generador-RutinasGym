using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.ValueObjects;

namespace GymRoutineGenerator.Domain.Services;

/// <summary>
/// Servicio de dominio para seleccionar ejercicios apropiados
/// </summary>
public interface IExerciseSelector
{
    /// <summary>
    /// Selecciona ejercicios apropiados para un grupo muscular y nivel de dificultad
    /// </summary>
    Task<IEnumerable<Exercise>> SelectExercisesForMuscleGroupAsync(
        MuscleGroup muscleGroup,
        DifficultyLevel userLevel,
        int count,
        IEnumerable<string>? userLimitations = null);

    /// <summary>
    /// Selecciona ejercicios balanceados para una rutina completa
    /// </summary>
    Task<IEnumerable<Exercise>> SelectBalancedRoutineExercisesAsync(
        IEnumerable<MuscleGroup> targetMuscles,
        DifficultyLevel userLevel,
        IEnumerable<EquipmentType> availableEquipment,
        IEnumerable<string>? userLimitations = null);

    /// <summary>
    /// Selecciona ejercicios para un plan de entrenamiento completo
    /// </summary>
    Task<Dictionary<int, IEnumerable<Exercise>>> SelectExercisesForWorkoutPlanAsync(
        WorkoutPlan plan,
        IEnumerable<EquipmentType> availableEquipment);

    /// <summary>
    /// Encuentra ejercicios alternativos para un ejercicio dado
    /// </summary>
    Task<IEnumerable<Exercise>> FindAlternativeExercisesAsync(
        Exercise exercise,
        DifficultyLevel? targetLevel = null);
}
