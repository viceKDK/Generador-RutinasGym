using System.Collections.Generic;

namespace GymRoutineGenerator.Domain;

/// <summary>
/// Lightweight profile used by the WinForms UI to collect user preferences before
/// sending them to the AI-driven routine generator.
/// </summary>
public class UserProfile
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string FitnessLevel { get; set; } = string.Empty;
    public int TrainingDays { get; set; }
    public List<string> Goals { get; set; } = new();
    public string PreferredTrainingTime { get; set; } = string.Empty;
    public bool HasInjuries { get; set; }
    public string Notes { get; set; } = string.Empty;
}
