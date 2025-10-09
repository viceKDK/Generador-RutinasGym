using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.Repositories;
using GymRoutineGenerator.Domain.Services;
using GymRoutineGenerator.Domain.ValueObjects;

namespace GymRoutineGenerator.Infrastructure.DomainServices;

/// <summary>
/// Implementación del servicio de selección de ejercicios
/// </summary>
public class ExerciseSelector : IExerciseSelector
{
    private readonly IExerciseRepository _exerciseRepository;
    private readonly IRoutineSafetyValidator _safetyValidator;

    public ExerciseSelector(
        IExerciseRepository exerciseRepository,
        IRoutineSafetyValidator safetyValidator)
    {
        _exerciseRepository = exerciseRepository;
        _safetyValidator = safetyValidator;
    }

    public async Task<IEnumerable<Exercise>> SelectExercisesForMuscleGroupAsync(
        MuscleGroup muscleGroup,
        DifficultyLevel userLevel,
        int count,
        IEnumerable<string>? userLimitations = null)
    {
        // Obtener todos los ejercicios para el grupo muscular
        var allExercises = await _exerciseRepository.GetByMuscleGroupAsync(muscleGroup);

        // Filtrar por nivel de dificultad
        var appropriateExercises = allExercises
            .Where(e => e.IsAppropriateForLevel(userLevel))
            .ToList();

        // Filtrar por seguridad si hay limitaciones
        if (userLimitations != null && userLimitations.Any())
        {
            appropriateExercises = appropriateExercises
                .Where(e => _safetyValidator.IsExerciseSafeForUser(e, userLimitations))
                .ToList();
        }

        // Seleccionar los primeros N ejercicios
        // En una implementación más sofisticada, podrías randomizar o usar criterios adicionales
        return appropriateExercises.Take(count);
    }

    public async Task<IEnumerable<Exercise>> SelectBalancedRoutineExercisesAsync(
        IEnumerable<MuscleGroup> targetMuscles,
        DifficultyLevel userLevel,
        IEnumerable<EquipmentType> availableEquipment,
        IEnumerable<string>? userLimitations = null)
    {
        var selectedExercises = new List<Exercise>();
        var muscleGroups = targetMuscles.ToList();

        foreach (var muscle in muscleGroups)
        {
            // Obtener ejercicios para este grupo muscular
            var allExercises = await _exerciseRepository.GetByMuscleGroupAsync(muscle);

            // Filtrar por nivel y equipo disponible
            var appropriateExercises = allExercises
                .Where(e => e.IsAppropriateForLevel(userLevel))
                .Where(e => availableEquipment.Any(eq => eq.Name == e.Equipment.Name))
                .ToList();

            // Filtrar por seguridad
            if (userLimitations != null && userLimitations.Any())
            {
                appropriateExercises = appropriateExercises
                    .Where(e => _safetyValidator.IsExerciseSafeForUser(e, userLimitations))
                    .ToList();
            }

            // Seleccionar 1-2 ejercicios por grupo muscular
            var exercisesForMuscle = appropriateExercises.Take(2);
            selectedExercises.AddRange(exercisesForMuscle);
        }

        return selectedExercises;
    }

    public async Task<Dictionary<int, IEnumerable<Exercise>>> SelectExercisesForWorkoutPlanAsync(
        WorkoutPlan plan,
        IEnumerable<EquipmentType> availableEquipment)
    {
        var result = new Dictionary<int, IEnumerable<Exercise>>();

        // Para cada día de entrenamiento, seleccionar ejercicios
        for (int day = 1; day <= plan.TrainingDaysPerWeek; day++)
        {
            // Estrategia simple: alternar entre tren superior e inferior
            var targetMuscles = day % 2 == 0
                ? GetUpperBodyMuscles()
                : GetLowerBodyMuscles();

            var exercises = await SelectBalancedRoutineExercisesAsync(
                targetMuscles,
                plan.UserLevel,
                availableEquipment,
                plan.UserLimitations);

            result[day] = exercises;
        }

        return result;
    }

    public async Task<IEnumerable<Exercise>> FindAlternativeExercisesAsync(
        Exercise exercise,
        DifficultyLevel? targetLevel = null)
    {
        // Buscar ejercicios que trabajen los mismos músculos
        var alternatives = new List<Exercise>();

        foreach (var muscle in exercise.TargetMuscles)
        {
            var muscleExercises = await _exerciseRepository.GetByMuscleGroupAsync(muscle);

            // Filtrar ejercicios diferentes al original
            var filtered = muscleExercises
                .Where(e => e.Id != exercise.Id);

            // Filtrar por nivel si se especifica
            if (targetLevel != null)
            {
                filtered = filtered.Where(e => e.IsAppropriateForLevel(targetLevel));
            }

            alternatives.AddRange(filtered);
        }

        // Eliminar duplicados
        return alternatives.DistinctBy(e => e.Id);
    }

    private List<MuscleGroup> GetUpperBodyMuscles()
    {
        return new List<MuscleGroup>
        {
            MuscleGroup.Pecho,
            MuscleGroup.Espalda,
            MuscleGroup.Hombros,
            MuscleGroup.Biceps,
            MuscleGroup.Triceps
        };
    }

    private List<MuscleGroup> GetLowerBodyMuscles()
    {
        return new List<MuscleGroup>
        {
            MuscleGroup.Cuadriceps,
            MuscleGroup.Isquiotibiales,
            MuscleGroup.Gluteos,
            MuscleGroup.Pantorrillas
        };
    }
}
