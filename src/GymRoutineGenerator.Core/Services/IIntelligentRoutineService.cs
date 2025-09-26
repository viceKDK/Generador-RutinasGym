namespace GymRoutineGenerator.Core.Services;

public interface IIntelligentRoutineService
{
    Task<RoutineGenerationResult> GenerateIntelligentRoutineAsync(int userProfileId, CancellationToken cancellationToken = default);
    Task<RoutineGenerationResult> GenerateIntelligentRoutineAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default);
    Task<RoutineGenerationResult> GenerateAlternativeRoutineAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default);
    Task<List<ExerciseAlternative>> GetExerciseAlternativesAsync(string exerciseName, UserRoutineParameters parameters, CancellationToken cancellationToken = default);
}

public class RoutineGenerationResult
{
    public bool IsSuccess { get; set; }
    public string? GeneratedRoutine { get; set; }
    public string? ErrorMessage { get; set; }
    public RoutineMetadata? Metadata { get; set; }
    public GenerationSource Source { get; set; }
    public TimeSpan GenerationTime { get; set; }
    public List<string> Warnings { get; set; } = new();
}

public class RoutineMetadata
{
    public string UserName { get; set; } = string.Empty;
    public string PrimaryGoal { get; set; } = string.Empty;
    public int EstimatedDuration { get; set; }
    public int DifficultyLevel { get; set; }
    public List<string> MuscleGroupsCovered { get; set; } = new();
    public List<string> EquipmentUsed { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string PromptUsed { get; set; } = string.Empty;
}

public class ExerciseAlternative
{
    public string ExerciseName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> MuscleGroups { get; set; } = new();
    public string EquipmentRequired { get; set; } = string.Empty;
    public int DifficultyLevel { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public enum GenerationSource
{
    AI_Generated,
    Fallback_Rules,
    Template_Based,
    Error_Occurred
}