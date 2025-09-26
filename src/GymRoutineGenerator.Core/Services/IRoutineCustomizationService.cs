using GymRoutineGenerator.Core.Models;

namespace GymRoutineGenerator.Core.Services;

public interface IRoutineCustomizationService
{
    Task<CustomizedRoutine> CreateCustomizedRoutineAsync(CustomizationRequest request, CancellationToken cancellationToken = default);
    Task<List<RoutineVariation>> GenerateRoutineVariationsAsync(BaseRoutine baseRoutine, VariationOptions options, CancellationToken cancellationToken = default);
    Task<AdaptedRoutine> AdaptRoutineToConstraintsAsync(BaseRoutine routine, ConstraintSet constraints, CancellationToken cancellationToken = default);
    Task<PersonalizedProgram> CreatePersonalizedProgramAsync(UserProfile userProfile, ProgramGoals goals, CancellationToken cancellationToken = default);
    Task<List<ExerciseSubstitution>> GetExerciseSubstitutionsAsync(string exerciseName, SubstitutionCriteria criteria, CancellationToken cancellationToken = default);
}

public class CustomizationRequest
{
    public UserProfile UserProfile { get; set; } = new();
    public RoutinePreferences Preferences { get; set; } = new();
    public List<CustomizationRule> CustomRules { get; set; } = new();
    public PrioritySettings Priorities { get; set; } = new();
    public EnvironmentConstraints Environment { get; set; } = new();
    public ProgressionPreferences Progression { get; set; } = new();
}

public class UserProfile
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public double Weight { get; set; }
    public double Height { get; set; }
    public string ExperienceLevel { get; set; } = "Principiante";
    public List<string> InjuryHistory { get; set; } = new();
    public List<string> PhysicalLimitations { get; set; } = new();
    public List<string> Medications { get; set; } = new();
    public string ActivityLevel { get; set; } = "Sedentario"; // Sedentario, Ligero, Moderado, Activo, Muy Activo
    public DateTime LastMedicalCheckup { get; set; }
    public List<BiometricData> BiometricHistory { get; set; } = new();
    public NutritionalProfile NutritionalInfo { get; set; } = new();
}

public class BiometricData
{
    public DateTime Date { get; set; }
    public double Weight { get; set; }
    public double BodyFatPercentage { get; set; }
    public double MuscleMassPercentage { get; set; }
    public int RestingHeartRate { get; set; }
    public string BloodPressure { get; set; } = string.Empty;
    public Dictionary<string, double> CustomMeasurements { get; set; } = new();
}

public class NutritionalProfile
{
    public int DailyCalories { get; set; }
    public double ProteinGrams { get; set; }
    public double CarbsGrams { get; set; }
    public double FatsGrams { get; set; }
    public List<string> DietaryRestrictions { get; set; } = new();
    public List<string> Allergies { get; set; } = new();
    public string EatingPattern { get; set; } = "Regular"; // Regular, Intermittent Fasting, etc.
}

public class RoutinePreferences
{
    public TimeSpan PreferredWorkoutDuration { get; set; }
    public TimeSpan MaxWorkoutDuration { get; set; }
    public List<string> PreferredTimeSlots { get; set; } = new(); // "Morning", "Afternoon", "Evening"
    public int PreferredDaysPerWeek { get; set; }
    public List<string> PreferredExerciseTypes { get; set; } = new(); // "Compound", "Isolation", "Cardio", "Flexibility"
    public List<string> PreferredMuscleGroupFocus { get; set; } = new();
    public List<string> DislikedExercises { get; set; } = new();
    public List<string> FavoriteExercises { get; set; } = new();
    public string IntensityPreference { get; set; } = "Moderada"; // Baja, Moderada, Alta, Variable
    public bool PrefersSupersets { get; set; }
    public bool PrefersCircuitTraining { get; set; }
    public bool WantsCardioIntegration { get; set; }
    public bool WantsFlexibilityWork { get; set; }
    public string MusicPreference { get; set; } = string.Empty;
    public bool WantsProgressTracking { get; set; } = true;
}

public class CustomizationRule
{
    public string RuleId { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty; // "Mandatory", "Preferred", "Avoided"
    public string Condition { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public int Priority { get; set; } = 1; // 1-10 scale
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class PrioritySettings
{
    public int SafetyPriority { get; set; } = 10; // 1-10 scale
    public int EffectivenessPriority { get; set; } = 8;
    public int ConveniencePriority { get; set; } = 6;
    public int VarietyPriority { get; set; } = 5;
    public int FunFactorPriority { get; set; } = 4;
    public int TimePriority { get; set; } = 7;
    public int EquipmentPriority { get; set; } = 6;
    public Dictionary<string, int> CustomPriorities { get; set; } = new();
}

public class EnvironmentConstraints
{
    public string WorkoutLocation { get; set; } = "Casa"; // Casa, Gimnasio, Parque, Oficina
    public List<string> AvailableEquipment { get; set; } = new();
    public Dictionary<string, int> EquipmentQuantity { get; set; } = new(); // Equipment name -> quantity
    public double AvailableSpace { get; set; } // in square meters
    public string SpaceType { get; set; } = string.Empty; // "Indoor", "Outdoor", "Mixed"
    public bool HasMirror { get; set; }
    public bool HasSound { get; set; }
    public string NoiseConstraints { get; set; } = "Ninguna"; // Ninguna, Baja, Muy Baja
    public string TemperatureConditions { get; set; } = "Controlada"; // Fría, Templada, Caliente, Controlada
    public List<string> SafetyFeatures { get; set; } = new(); // "Spotter Available", "Emergency Access", etc.
}

public class ProgressionPreferences
{
    public string ProgressionStyle { get; set; } = "Linear"; // Linear, Undulating, Block, AutoRegulated
    public int WeeksPerPhase { get; set; } = 4;
    public string ProgressionMetric { get; set; } = "Volume"; // Volume, Intensity, Complexity, Time
    public double ProgressionRate { get; set; } = 0.05; // 5% increase per progression
    public bool WantsDeloadWeeks { get; set; } = true;
    public int DeloadFrequency { get; set; } = 4; // Every 4 weeks
    public List<string> TestingMethods { get; set; } = new(); // "1RM Test", "Rep Max", "Time Trial"
    public bool WantsPeriodization { get; set; } = true;
}

public class CustomizedRoutine
{
    public string RoutineId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string RoutineName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan EstimatedDuration { get; set; }

    public CustomizedWarmup Warmup { get; set; } = new();
    public List<CustomizedWorkoutBlock> WorkoutBlocks { get; set; } = new();
    public CustomizedCooldown Cooldown { get; set; } = new();

    public CustomizationMetadata Metadata { get; set; } = new();
    public List<PersonalizationNote> PersonalizationNotes { get; set; } = new();
    public AdaptationSummary Adaptations { get; set; } = new();
    public ProgressionPlan ProgressionPlan { get; set; } = new();
}

public class CustomizedWarmup
{
    public TimeSpan Duration { get; set; }
    public List<CustomizedWarmupPhase> Phases { get; set; } = new();
    public string PersonalizationReason { get; set; } = string.Empty;
    public List<string> SpecialConsiderations { get; set; } = new();
}

public class CustomizedWarmupPhase
{
    public string PhaseName { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public List<CustomizedExercise> Exercises { get; set; } = new();
    public string Instructions { get; set; } = string.Empty;
    public string AdaptationReason { get; set; } = string.Empty;
}

public class CustomizedWorkoutBlock
{
    public string BlockName { get; set; } = string.Empty;
    public string BlockType { get; set; } = string.Empty;
    public List<CustomizedExercise> Exercises { get; set; } = new();
    public string Purpose { get; set; } = string.Empty;
    public TimeSpan EstimatedTime { get; set; }
    public int OrderInWorkout { get; set; }
    public List<string> CustomizationReasons { get; set; } = new();
}

public class CustomizedExercise
{
    public string ExerciseId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AlternativeName { get; set; } = string.Empty;
    public List<string> MuscleGroups { get; set; } = new();
    public CustomizedParameters Parameters { get; set; } = new();
    public List<string> PersonalizedInstructions { get; set; } = new();
    public List<string> SafetyModifications { get; set; } = new();
    public List<string> ProgressionOptions { get; set; } = new();
    public string Equipment { get; set; } = string.Empty;
    public string CustomizationReason { get; set; } = string.Empty;
    public List<ExerciseAlternative> Alternatives { get; set; } = new();
}

public class CustomizedParameters
{
    public SetConfiguration Sets { get; set; } = new();
    public RepConfiguration Reps { get; set; } = new();
    public IntensityConfiguration Intensity { get; set; } = new();
    public RestConfiguration Rest { get; set; } = new();
    public TempoConfiguration Tempo { get; set; } = new();
    public string LoadRecommendation { get; set; } = string.Empty;
    public List<string> ProgressionNotes { get; set; } = new();
}

public class SetConfiguration
{
    public int BaseCount { get; set; }
    public int MinimumCount { get; set; }
    public int MaximumCount { get; set; }
    public bool IsVariable { get; set; }
    public string VariationPattern { get; set; } = string.Empty;
    public List<string> AdaptationNotes { get; set; } = new();
}

public class RepConfiguration
{
    public int TargetReps { get; set; }
    public int MinimumReps { get; set; }
    public int MaximumReps { get; set; }
    public bool IsTimeBasedReps { get; set; }
    public int TimeInSeconds { get; set; }
    public string RepStyle { get; set; } = "Standard"; // Standard, Cluster, Rest-Pause, Drop
    public List<string> PersonalizationNotes { get; set; } = new();
}

public class IntensityConfiguration
{
    public int RPETarget { get; set; } // Rating of Perceived Exertion
    public int RPERange { get; set; } = 1; // ±1 RPE
    public double PercentageOfMax { get; set; }
    public string IntensityMethod { get; set; } = "RPE"; // RPE, Percentage, Autoregulated
    public List<string> IntensityModifications { get; set; } = new();
}

public class RestConfiguration
{
    public TimeSpan TargetRest { get; set; }
    public TimeSpan MinimumRest { get; set; }
    public TimeSpan MaximumRest { get; set; }
    public string RestType { get; set; } = "Passive"; // Passive, Active, Superset
    public string ActivityDuringRest { get; set; } = string.Empty;
    public List<string> RestModifications { get; set; } = new();
}

public class TempoConfiguration
{
    public string TempoNotation { get; set; } = "2-0-2-0"; // Eccentric-Pause-Concentric-Top
    public bool IsControlledTempo { get; set; } = true;
    public string TempoFocus { get; set; } = "Control"; // Control, Power, Endurance
    public List<string> TempoAdjustments { get; set; } = new();
}

public class CustomizedCooldown
{
    public TimeSpan Duration { get; set; }
    public List<CustomizedCooldownPhase> Phases { get; set; } = new();
    public string PersonalizationReason { get; set; } = string.Empty;
    public List<string> RecoveryTips { get; set; } = new();
}

public class CustomizedCooldownPhase
{
    public string PhaseName { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public List<CustomizedExercise> Exercises { get; set; } = new();
    public string Instructions { get; set; } = string.Empty;
    public string AdaptationReason { get; set; } = string.Empty;
}

public class CustomizationMetadata
{
    public string CustomizationVersion { get; set; } = "1.0";
    public List<string> AppliedRules { get; set; } = new();
    public Dictionary<string, object> CustomizationParameters { get; set; } = new();
    public double PersonalizationScore { get; set; } // 0.0-1.0 scale
    public List<string> SafetyAdaptations { get; set; } = new();
    public List<string> PreferenceAdaptations { get; set; } = new();
    public List<string> ConstraintAdaptations { get; set; } = new();
}

public class PersonalizationNote
{
    public string Category { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public NotePriority Priority { get; set; }
    public bool RequiresUserAttention { get; set; }
}

public class AdaptationSummary
{
    public List<string> MajorAdaptations { get; set; } = new();
    public List<string> MinorAdaptations { get; set; } = new();
    public List<string> SafetyModifications { get; set; } = new();
    public List<string> PreferenceAccommodations { get; set; } = new();
    public Dictionary<string, string> SubstitutionReasons { get; set; } = new();
}

public class BaseRoutine
{
    public string RoutineId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Exercise> Exercises { get; set; } = new();
    public TimeSpan EstimatedDuration { get; set; }
    public string DifficultyLevel { get; set; } = string.Empty;
    public List<string> TargetMuscleGroups { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class RoutineVariation
{
    public string VariationId { get; set; } = Guid.NewGuid().ToString();
    public string VariationName { get; set; } = string.Empty;
    public string VariationType { get; set; } = string.Empty; // "Equipment", "Difficulty", "Duration", "Focus"
    public BaseRoutine ModifiedRoutine { get; set; } = new();
    public List<string> Changes { get; set; } = new();
    public string VariationReason { get; set; } = string.Empty;
    public double SimilarityScore { get; set; } // 0.0-1.0 similarity to base routine
    public List<string> Benefits { get; set; } = new();
    public List<string> Considerations { get; set; } = new();
}

public class VariationOptions
{
    public List<string> VariationTypes { get; set; } = new(); // Types of variations to generate
    public int MaxVariations { get; set; } = 5;
    public double MinSimilarityScore { get; set; } = 0.6; // Minimum similarity to base routine
    public List<string> PreferredFocus { get; set; } = new();
    public bool AllowEquipmentChanges { get; set; } = true;
    public bool AllowDifficultyChanges { get; set; } = true;
    public bool AllowDurationChanges { get; set; } = true;
    public Dictionary<string, object> CustomOptions { get; set; } = new();
}

public class AdaptedRoutine
{
    public string AdaptedRoutineId { get; set; } = Guid.NewGuid().ToString();
    public BaseRoutine OriginalRoutine { get; set; } = new();
    public BaseRoutine AdaptedRoutine_ { get; set; } = new();
    public ConstraintSet AppliedConstraints { get; set; } = new();
    public List<AdaptationDetail> Adaptations { get; set; } = new();
    public double AdaptationScore { get; set; } // 0.0-1.0 how well constraints were accommodated
    public List<string> LimitationsNotAddressed { get; set; } = new();
}

public class ConstraintSet
{
    public List<PhysicalConstraint> PhysicalConstraints { get; set; } = new();
    public List<EquipmentConstraint> EquipmentConstraints { get; set; } = new();
    public List<TimeConstraint> TimeConstraints { get; set; } = new();
    public List<SafetyConstraint> SafetyConstraints { get; set; } = new();
    public List<PreferenceConstraint> PreferenceConstraints { get; set; } = new();
}

public class PhysicalConstraint
{
    public string ConstraintType { get; set; } = string.Empty; // "Injury", "Limitation", "Condition"
    public string Description { get; set; } = string.Empty;
    public List<string> AffectedMovements { get; set; } = new();
    public List<string> RestrictedExercises { get; set; } = new();
    public List<string> RecommendedModifications { get; set; } = new();
    public ConstraintSeverity Severity { get; set; }
}

public class EquipmentConstraint
{
    public List<string> AvailableEquipment { get; set; } = new();
    public List<string> UnavailableEquipment { get; set; } = new();
    public Dictionary<string, int> QuantityLimitations { get; set; } = new();
    public string SubstitutionStrategy { get; set; } = "Automatic";
}

public class TimeConstraint
{
    public TimeSpan MaxWorkoutDuration { get; set; }
    public TimeSpan PreferredDuration { get; set; }
    public List<string> AvailableTimeSlots { get; set; } = new();
    public bool AllowSplitSessions { get; set; } = true;
    public TimeSpan MaxContinuousTime { get; set; }
}

public class SafetyConstraint
{
    public List<string> ProhibitedMovements { get; set; } = new();
    public List<string> RequiredSupervision { get; set; } = new();
    public int MaxHeartRateLimit { get; set; }
    public List<string> MandatoryWarmupElements { get; set; } = new();
    public List<string> MandatoryCooldownElements { get; set; } = new();
}

public class PreferenceConstraint
{
    public List<string> PreferredExercises { get; set; } = new();
    public List<string> DislikedExercises { get; set; } = new();
    public string PreferredIntensity { get; set; } = string.Empty;
    public List<string> PreferredMuscleGroups { get; set; } = new();
    public bool PreferCompoundMovements { get; set; } = true;
}

public class AdaptationDetail
{
    public string AdaptationType { get; set; } = string.Empty;
    public string OriginalElement { get; set; } = string.Empty;
    public string AdaptedElement { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public double ImpactScore { get; set; } // 0.0-1.0 impact on routine effectiveness
}

public class PersonalizedProgram
{
    public string ProgramId { get; set; } = Guid.NewGuid().ToString();
    public UserProfile UserProfile { get; set; } = new();
    public ProgramGoals Goals { get; set; } = new();
    public List<ProgramPhase> Phases { get; set; } = new();
    public TimeSpan TotalDuration { get; set; }
    public DateTime StartDate { get; set; }
    public ProgressTrackingPlan TrackingPlan { get; set; } = new();
    public List<ProgramMilestone> Milestones { get; set; } = new();
}

public class ProgramGoals
{
    public string PrimaryGoal { get; set; } = string.Empty;
    public List<string> SecondaryGoals { get; set; } = new();
    public Dictionary<string, MeasurableTarget> QuantifiableTargets { get; set; } = new();
    public DateTime TargetDate { get; set; }
    public List<string> MotivationalFactors { get; set; } = new();
}

public class MeasurableTarget
{
    public string Metric { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double TargetValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string MeasurementMethod { get; set; } = string.Empty;
}

public class ProgramPhase
{
    public string PhaseName { get; set; } = string.Empty;
    public int PhaseNumber { get; set; }
    public TimeSpan Duration { get; set; }
    public string Focus { get; set; } = string.Empty;
    public List<CustomizedRoutine> Routines { get; set; } = new();
    public PhaseTransition TransitionPlan { get; set; } = new();
}

public class PhaseTransition
{
    public string TransitionType { get; set; } = string.Empty;
    public TimeSpan TransitionDuration { get; set; }
    public List<string> TransitionActivities { get; set; } = new();
    public string NextPhasePreparation { get; set; } = string.Empty;
}

public class ProgressTrackingPlan
{
    public List<string> TrackingMetrics { get; set; } = new();
    public int AssessmentFrequency { get; set; } = 7; // Days
    public List<AssessmentMethod> AssessmentMethods { get; set; } = new();
    public string ProgressVisualization { get; set; } = string.Empty;
}

public class AssessmentMethod
{
    public string MethodName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int FrequencyDays { get; set; }
    public List<string> RequiredEquipment { get; set; } = new();
    public string Instructions { get; set; } = string.Empty;
}

public class ProgramMilestone
{
    public string MilestoneName { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public List<string> SuccessCriteria { get; set; } = new();
    public string CelebrationPlan { get; set; } = string.Empty;
    public List<string> RewardOptions { get; set; } = new();
}

public class ExerciseSubstitution
{
    public string OriginalExercise { get; set; } = string.Empty;
    public string SubstituteExercise { get; set; } = string.Empty;
    public string SubstitutionReason { get; set; } = string.Empty;
    public double SimilarityScore { get; set; } // 0.0-1.0
    public List<string> SimilarMuscleGroups { get; set; } = new();
    public List<string> Differences { get; set; } = new();
    public string EquipmentRequired { get; set; } = string.Empty;
    public string DifficultyComparison { get; set; } = string.Empty;
    public List<string> ModificationNotes { get; set; } = new();
}

public class SubstitutionCriteria
{
    public List<string> RequiredMuscleGroups { get; set; } = new();
    public List<string> AvailableEquipment { get; set; } = new();
    public string MaxDifficulty { get; set; } = string.Empty;
    public List<string> MovementPatterns { get; set; } = new();
    public List<string> AvoidedMovements { get; set; } = new();
    public bool MaintainIntensity { get; set; } = true;
    public double MinSimilarityScore { get; set; } = 0.7;
}

// Enums
public enum NotePriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum ConstraintSeverity
{
    Mild = 1,
    Moderate = 2,
    Severe = 3,
    Absolute = 4
}