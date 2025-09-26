using GymRoutineGenerator.Core.Models;

namespace GymRoutineGenerator.Core.Services;

public interface IRoutineStructureService
{
    Task<StructuredRoutine> CreateStructuredRoutineAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default);
    Task<List<WorkoutSession>> GenerateWeeklyProgramAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default);
    Task<ExerciseSequence> OptimizeExerciseOrderAsync(List<Exercise> exercises, UserRoutineParameters parameters, CancellationToken cancellationToken = default);
    Task<TrainingVolume> CalculateOptimalVolumeAsync(UserRoutineParameters parameters, CancellationToken cancellationToken = default);
}

public class StructuredRoutine
{
    public string RoutineName { get; set; } = string.Empty;
    public UserRoutineParameters UserParameters { get; set; } = new();
    public WarmupProtocol Warmup { get; set; } = new();
    public List<TrainingBlock> MainWorkout { get; set; } = new();
    public CooldownProtocol Cooldown { get; set; } = new();
    public TrainingVolume Volume { get; set; } = new();
    public ProgressionPlan Progression { get; set; } = new();
    public List<SafetyConsideration> SafetyNotes { get; set; } = new();
    public TimeSpan EstimatedDuration { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class WorkoutSession
{
    public int DayNumber { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public List<string> TargetMuscleGroups { get; set; } = new();
    public WarmupProtocol Warmup { get; set; } = new();
    public List<StructuredExercise> Exercises { get; set; } = new();
    public CooldownProtocol Cooldown { get; set; } = new();
    public TimeSpan EstimatedDuration { get; set; }
    public IntensityLevel TargetIntensity { get; set; }
    public List<string> SpecialNotes { get; set; } = new();
}

public class StructuredExercise
{
    public Exercise BaseExercise { get; set; } = new();
    public int OrderInWorkout { get; set; }
    public TrainingParameters Parameters { get; set; } = new();
    public List<ExerciseModification> Modifications { get; set; } = new();
    public List<string> TechniqueNotes { get; set; } = new();
    public RestPeriod RestPeriod { get; set; } = new();
    public ProgressionScheme Progression { get; set; } = new();
}

public class TrainingParameters
{
    public int Sets { get; set; }
    public RepRange Reps { get; set; } = new();
    public IntensityRange Intensity { get; set; } = new();
    public TempoPrescription Tempo { get; set; } = new();
    public bool IsDropSet { get; set; }
    public bool IsSuperSet { get; set; }
    public string SuperSetPartner { get; set; } = string.Empty;
    public double LoadPercentage { get; set; } // Percentage of 1RM if applicable
}

public class RepRange
{
    public int Minimum { get; set; }
    public int Maximum { get; set; }
    public int Target { get; set; }
    public string RangeDescription => $"{Minimum}-{Maximum}";
    public bool IsTimeBasedReps { get; set; } // For holds/isometric exercises
    public int TimeInSeconds { get; set; }
}

public class IntensityRange
{
    public int RPEMinimum { get; set; } // Rating of Perceived Exertion
    public int RPEMaximum { get; set; }
    public int RPETarget { get; set; }
    public double PercentageOfMax { get; set; }
    public string IntensityDescription { get; set; } = string.Empty;
}

public class TempoPrescription
{
    public int EccentricSeconds { get; set; } // Lowering phase
    public int PauseSeconds { get; set; } // Bottom pause
    public int ConcentricSeconds { get; set; } // Lifting phase
    public int TopPauseSeconds { get; set; } // Top pause
    public string TempoNotation => $"{EccentricSeconds}{PauseSeconds}{ConcentricSeconds}{TopPauseSeconds}";
    public bool IsControlledTempo { get; set; }
}

public class RestPeriod
{
    public TimeSpan MinimumRest { get; set; }
    public TimeSpan MaximumRest { get; set; }
    public TimeSpan TargetRest { get; set; }
    public RestType Type { get; set; }
    public string ActivityDuringRest { get; set; } = string.Empty;
    public bool IsActiveRest { get; set; }
}

public class ExerciseSequence
{
    public List<StructuredExercise> OrderedExercises { get; set; } = new();
    public List<ExerciseTransition> Transitions { get; set; } = new();
    public string SequenceRationale { get; set; } = string.Empty;
    public List<string> OptimizationNotes { get; set; } = new();
}

public class ExerciseTransition
{
    public int FromExerciseIndex { get; set; }
    public int ToExerciseIndex { get; set; }
    public TimeSpan TransitionTime { get; set; }
    public string TransitionActivity { get; set; } = string.Empty;
    public bool RequiresEquipmentChange { get; set; }
}

public class TrainingVolume
{
    public int TotalSets { get; set; }
    public int TotalReps { get; set; }
    public TimeSpan TotalWorkTime { get; set; }
    public TimeSpan TotalRestTime { get; set; }
    public Dictionary<string, int> SetsPerMuscleGroup { get; set; } = new();
    public double VolumeLoad { get; set; } // Sets x Reps x Weight if applicable
    public VolumeClassification Classification { get; set; }
    public List<string> VolumeRecommendations { get; set; } = new();
}

public class WarmupProtocol
{
    public TimeSpan Duration { get; set; }
    public List<WarmupPhase> Phases { get; set; } = new();
    public string Purpose { get; set; } = string.Empty;
    public List<string> SpecialConsiderations { get; set; } = new();
}

public class WarmupPhase
{
    public string PhaseName { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public List<Exercise> Exercises { get; set; } = new();
    public string Instructions { get; set; } = string.Empty;
    public IntensityLevel TargetIntensity { get; set; }
}

public class CooldownProtocol
{
    public TimeSpan Duration { get; set; }
    public List<CooldownPhase> Phases { get; set; } = new();
    public string Purpose { get; set; } = string.Empty;
    public List<string> RecoveryTips { get; set; } = new();
}

public class CooldownPhase
{
    public string PhaseName { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public List<Exercise> Exercises { get; set; } = new();
    public string Instructions { get; set; } = string.Empty;
    public bool IsStretching { get; set; }
    public bool IsMobilityWork { get; set; }
}

public class ProgressionPlan
{
    public List<ProgressionWeek> Weeks { get; set; } = new();
    public ProgressionStrategy Strategy { get; set; }
    public List<string> ProgressionNotes { get; set; } = new();
    public List<ProgressionMarker> Milestones { get; set; } = new();
}

public class ProgressionWeek
{
    public int WeekNumber { get; set; }
    public string Focus { get; set; } = string.Empty;
    public Dictionary<string, object> ParameterAdjustments { get; set; } = new();
    public List<string> ExpectedAdaptations { get; set; } = new();
}

public class ProgressionMarker
{
    public string Metric { get; set; } = string.Empty;
    public string TargetValue { get; set; } = string.Empty;
    public int TargetWeek { get; set; }
    public string AssessmentMethod { get; set; } = string.Empty;
}

public class TrainingBlock
{
    public string BlockName { get; set; } = string.Empty;
    public ExerciseCategory Category { get; set; }
    public List<StructuredExercise> Exercises { get; set; } = new();
    public string Purpose { get; set; } = string.Empty;
    public TimeSpan EstimatedDuration { get; set; }
    public int OrderInWorkout { get; set; }
}

public class SafetyConsideration
{
    public string Consideration { get; set; } = string.Empty;
    public SafetyLevel Severity { get; set; }
    public List<string> Precautions { get; set; } = new();
    public List<string> WarningSignsToStop { get; set; } = new();
}

public class ExerciseModification
{
    public string ModificationName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ModificationReason Reason { get; set; }
    public string Instructions { get; set; } = string.Empty;
}

public class ProgressionScheme
{
    public ProgressionType Type { get; set; }
    public Dictionary<string, object> ProgressionValues { get; set; } = new();
    public string ProgressionInstructions { get; set; } = string.Empty;
    public int WeeksUntilProgresssion { get; set; }
}

// Enums
public enum IntensityLevel
{
    VeryLight = 1,
    Light = 2,
    Moderate = 3,
    Vigorous = 4,
    VeryVigorous = 5
}

public enum RestType
{
    Complete,
    Active,
    Passive,
    Superset,
    Circuit
}

public enum VolumeClassification
{
    Low,
    Moderate,
    High,
    VeryHigh
}

public enum ExerciseCategory
{
    Warmup,
    Compound,
    Isolation,
    Core,
    Cardio,
    Flexibility,
    Cooldown,
    Accessory
}

public enum SafetyLevel
{
    Info,
    Caution,
    Warning,
    Critical
}

public enum ModificationReason
{
    PhysicalLimitation,
    EquipmentSubstitution,
    SkillLevel,
    Safety,
    Preference
}

public enum ProgressionStrategy
{
    Linear,
    Undulating,
    BlockPeriodization,
    Conjugate,
    AutoRegulated
}

public enum ProgressionType
{
    VolumeIncrease,
    IntensityIncrease,
    FrequencyIncrease,
    ComplexityIncrease,
    SkillRefinement
}