namespace GymRoutineGenerator.Core.Models;

public class Exercise
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SpanishName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public List<string> MuscleGroups { get; set; } = new();
    public string Equipment { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = "Intermedio";
    public ExerciseType ExerciseType { get; set; } = ExerciseType.Compound;
    public string Type { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();

    // Training parameters
    public int RecommendedSets { get; set; } = 3;
    public string RecommendedReps { get; set; } = "8-12";
    public string RestPeriod { get; set; } = "60-90 seconds";

    // Additional properties
    public List<string> Modifications { get; set; } = new();
    public List<string> SafetyNotes { get; set; } = new();
}

public enum ExerciseType
{
    Compound,
    Isolation,
    Cardio,
    Flexibility,
    Balance,
    Core,
    Warmup,
    Cooldown
}