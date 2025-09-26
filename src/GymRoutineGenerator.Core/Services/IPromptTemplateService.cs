namespace GymRoutineGenerator.Core.Services;

public interface IPromptTemplateService
{
    Task<string> BuildIntelligentRoutinePromptAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default);
    Task<string> BuildExerciseSelectionPromptAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default);
    Task<string> BuildFallbackPromptAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default);
}

public class UserRoutineParameters
{
    // Demographics
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public int TrainingDaysPerWeek { get; set; }
    public string ExperienceLevel { get; set; } = "Principiante";

    // Equipment Available
    public List<string> AvailableEquipment { get; set; } = new();
    public string GymType { get; set; } = "Casa"; // Casa, Gimnasio, Parque

    // Muscle Group Preferences
    public List<MuscleGroupFocus> MuscleGroupPreferences { get; set; } = new();
    public string PrimaryGoal { get; set; } = "Fitness General"; // Fuerza, Masa, Resistencia, Pérdida de peso

    // Physical Limitations
    public List<string> PhysicalLimitations { get; set; } = new();
    public int RecommendedIntensity { get; set; } = 3; // 1-5 scale
    public List<string> AvoidExercises { get; set; } = new();

    // Preferences
    public int PreferredSessionDuration { get; set; } = 45; // minutes
    public List<string> PreferredExerciseTypes { get; set; } = new();
    public bool IncludeCardio { get; set; } = true;
    public bool IncludeFlexibility { get; set; } = true;
}

public class MuscleGroupFocus
{
    public string MuscleGroup { get; set; } = string.Empty;
    public string EmphasisLevel { get; set; } = "Medio"; // Alto, Medio, Bajo
    public int Priority { get; set; } = 1; // 1-5, where 1 is highest priority
}

public class RoutineContext
{
    public string TrainingPhase { get; set; } = "Adaptación"; // Adaptación, Construcción, Intensificación
    public int WeekInProgram { get; set; } = 1;
    public string SeasonalConsiderations { get; set; } = string.Empty;
    public List<string> RecentExercises { get; set; } = new(); // For variation
    public string SpecialFocus { get; set; } = string.Empty; // Rehabilitation, Competition prep, etc.
}