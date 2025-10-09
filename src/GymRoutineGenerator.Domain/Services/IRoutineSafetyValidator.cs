using GymRoutineGenerator.Domain.Aggregates;

namespace GymRoutineGenerator.Domain.Services;

/// <summary>
/// Servicio de dominio para validar la seguridad de una rutina
/// </summary>
public interface IRoutineSafetyValidator
{
    /// <summary>
    /// Valida si un ejercicio es seguro para un usuario con las limitaciones dadas
    /// </summary>
    bool IsExerciseSafeForUser(Exercise exercise, IEnumerable<string> userLimitations);

    /// <summary>
    /// Valida si una rutina completa es segura para un usuario
    /// </summary>
    bool IsRoutineSafeForUser(Routine routine, IEnumerable<string> userLimitations);

    /// <summary>
    /// Valida si un plan de entrenamiento completo es seguro
    /// </summary>
    ValidationResult ValidateWorkoutPlan(WorkoutPlan plan);

    /// <summary>
    /// Obtiene sugerencias de ejercicios alternativos seguros
    /// </summary>
    Task<IEnumerable<Exercise>> GetSafeAlternativesAsync(Exercise unsafeExercise, IEnumerable<string> userLimitations);
}

/// <summary>
/// Resultado de validación de seguridad
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();

    public static ValidationResult Success() => new ValidationResult { IsValid = true };

    public static ValidationResult Failure(params string[] errors)
    {
        return new ValidationResult
        {
            IsValid = false,
            Errors = errors.ToList()
        };
    }

    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }

    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }
}
