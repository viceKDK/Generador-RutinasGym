using System.Collections.Generic;

namespace GymRoutineGenerator.Core.Models;

/// <summary>
/// Contexto unificado de rutina empleado por servicios de prompts y memoria conversacional.
/// </summary>
public class RoutineContext
{
    // Datos de fase de entrenamiento
    public string TrainingPhase { get; set; } = "Adaptacion"; // Adaptacion, Construccion, Intensificacion
    public int WeekInProgram { get; set; } = 1;
    public string SeasonalConsiderations { get; set; } = string.Empty;
    public List<string> RecentExercises { get; set; } = new();
    public string SpecialFocus { get; set; } = string.Empty;

    // Datos de seguimiento conversacional
    public int? CurrentRoutineId { get; set; }
    public int ModificationCount { get; set; }
    public List<string> RecentModifications { get; set; } = new();
    public double UserSatisfactionLevel { get; set; }
}
