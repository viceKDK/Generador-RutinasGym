using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.Repositories;
using GymRoutineGenerator.Domain.Services;
using GymRoutineGenerator.Domain.ValueObjects;

namespace GymRoutineGenerator.Infrastructure.DomainServices;

/// <summary>
/// Implementación del servicio de dominio para validar seguridad de rutinas
/// </summary>
public class RoutineSafetyValidator : IRoutineSafetyValidator
{
    private readonly IExerciseRepository _exerciseRepository;

    // Mapeo de limitaciones a grupos musculares/movimientos afectados
    private static readonly Dictionary<string, HashSet<string>> LimitationImpacts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["rodilla"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Piernas", "Cuádriceps", "Gemelos", "Femorales" },
        ["espalda"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Espalda", "Lumbares", "Dorsales" },
        ["hombro"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Hombros", "Deltoides" },
        ["codo"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Bíceps", "Tríceps", "Antebrazos" },
        ["muñeca"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Antebrazos" },
        ["cuello"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Trapecio", "Cuello" },
        ["cadera"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Piernas", "Glúteos" },
        ["tobillo"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Gemelos", "Piernas" }
    };

    public RoutineSafetyValidator(IExerciseRepository exerciseRepository)
    {
        _exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
    }

    public bool IsExerciseSafeForUser(Exercise exercise, IEnumerable<string> userLimitations)
    {
        if (exercise == null || userLimitations == null || !userLimitations.Any())
            return true;

        foreach (var limitation in userLimitations)
        {
            if (LimitationImpacts.TryGetValue(limitation, out var affectedMuscles))
            {
                if (exercise.TargetMuscles.Any(m => affectedMuscles.Contains(m.SpanishName)))
                    return false;

                if (exercise.SecondaryMuscles.Any(m => affectedMuscles.Contains(m.SpanishName)))
                    return false;
            }
        }

        return true;
    }

    public bool IsRoutineSafeForUser(Routine routine, IEnumerable<string> userLimitations)
    {
        if (routine == null)
            throw new ArgumentNullException(nameof(routine));

        if (userLimitations == null || !userLimitations.Any())
            return true;

        foreach (var routineExercise in routine.Exercises)
        {
            if (!IsExerciseSafeForUser(routineExercise.Exercise, userLimitations))
                return false;
        }

        return true;
    }

    public ValidationResult ValidateWorkoutPlan(WorkoutPlan plan)
    {
        if (plan == null)
            throw new ArgumentNullException(nameof(plan));

        var result = ValidationResult.Success();

        if (!plan.Routines.Any())
        {
            result.AddError("El plan no tiene rutinas asignadas");
            return result;
        }

        if (plan.Routines.Count != plan.TrainingDaysPerWeek)
        {
            result.AddWarning($"El plan debería tener {plan.TrainingDaysPerWeek} rutinas pero tiene {plan.Routines.Count}");
        }

        foreach (var routine in plan.Routines)
        {
            if (!routine.Exercises.Any())
                result.AddError($"La rutina '{routine.Name}' no tiene ejercicios");
        }

        if (plan.UserLimitations.Any())
        {
            foreach (var routine in plan.Routines)
            {
                if (!IsRoutineSafeForUser(routine, plan.UserLimitations))
                    result.AddError($"La rutina '{routine.Name}' contiene ejercicios no seguros");
            }
        }

        var allTargetedMuscles = plan.Routines
            .SelectMany(r => r.GetTargetedMuscleGroups())
            .Distinct()
            .ToList();

        if (allTargetedMuscles.Count < 3)
            result.AddWarning("El plan trabaja menos de 3 grupos musculares principales");

        var totalExercises = plan.GetTotalExercises();
        var minExercisesRecommended = plan.TrainingDaysPerWeek * 3;

        if (totalExercises < minExercisesRecommended)
            result.AddWarning($"El plan tiene {totalExercises} ejercicios, se recomiendan al menos {minExercisesRecommended}");

        return result;
    }

    public async Task<IEnumerable<Exercise>> GetSafeAlternativesAsync(Exercise unsafeExercise, IEnumerable<string> userLimitations)
    {
        if (unsafeExercise == null)
            throw new ArgumentNullException(nameof(unsafeExercise));

        if (userLimitations == null || !userLimitations.Any())
            return Enumerable.Empty<Exercise>();

        var allExercises = await _exerciseRepository.GetActiveExercisesAsync();

        var safeAlternatives = allExercises
            .Where(e => e.Id != unsafeExercise.Id)
            .Where(e => e.Difficulty.Level == unsafeExercise.Difficulty.Level)
            .Where(e => e.TargetMuscles.Any(m1 => unsafeExercise.TargetMuscles.Any(m2 => m1.Name == m2.Name)))
            .Where(e => IsExerciseSafeForUser(e, userLimitations))
            .Take(5)
            .ToList();

        return safeAlternatives;
    }
}
