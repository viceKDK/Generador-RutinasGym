namespace GymRoutineGenerator.Core.Services;

public interface ISpanishResponseProcessor
{
    Task<ProcessedRoutineResponse> ProcessAIResponseAsync(string rawResponse, UserRoutineParameters parameters, CancellationToken cancellationToken = default);
    Task<SpanishValidationResult> ValidateSpanishContentAsync(string content, CancellationToken cancellationToken = default);
    Task<string> EnhanceSpanishFormattingAsync(string content, CancellationToken cancellationToken = default);
    Task<List<ExerciseInstruction>> ParseExerciseInstructionsAsync(string instructions, CancellationToken cancellationToken = default);
    Task<RoutineQualityScore> AssessResponseQualityAsync(string response, UserRoutineParameters parameters, CancellationToken cancellationToken = default);
}

public class ProcessedRoutineResponse
{
    public string ProcessedContent { get; set; } = string.Empty;
    public RoutineStructure Structure { get; set; } = new();
    public SpanishValidationResult Validation { get; set; } = new();
    public RoutineQualityScore Quality { get; set; } = new();
    public List<ProcessingWarning> Warnings { get; set; } = new();
    public List<string> Corrections { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
    public bool RequiresHumanReview { get; set; }
}

public class RoutineStructure
{
    public string Title { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public WarmupSection Warmup { get; set; } = new();
    public List<ExerciseBlock> ExerciseBlocks { get; set; } = new();
    public CooldownSection Cooldown { get; set; } = new();
    public NutritionalGuidance Nutrition { get; set; } = new();
    public List<SafetyNote> SafetyNotes { get; set; } = new();
    public ProgressionPlan ProgressionPlan { get; set; } = new();
    public TimeSpan EstimatedDuration { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class WarmupSection
{
    public TimeSpan Duration { get; set; }
    public List<WarmupExercise> Exercises { get; set; } = new();
    public string Purpose { get; set; } = string.Empty;
    public List<string> Instructions { get; set; } = new();
}

public class WarmupExercise
{
    public string Name { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Intensity { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public List<string> MuscleGroups { get; set; } = new();
}

public class ExerciseBlock
{
    public string BlockName { get; set; } = string.Empty;
    public string BlockType { get; set; } = string.Empty; // "Principal", "Accesorio", "Cardio"
    public List<DetailedExercise> Exercises { get; set; } = new();
    public string Purpose { get; set; } = string.Empty;
    public TimeSpan EstimatedTime { get; set; }
    public int OrderInRoutine { get; set; }
}

public class DetailedExercise
{
    public string Name { get; set; } = string.Empty;
    public string AlternativeName { get; set; } = string.Empty;
    public List<string> MuscleGroups { get; set; } = new();
    public ExerciseParameters Parameters { get; set; } = new();
    public List<string> StepByStepInstructions { get; set; } = new();
    public List<string> CommonMistakes { get; set; } = new();
    public List<string> SafetyTips { get; set; } = new();
    public string Equipment { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = string.Empty;
    public List<string> Modifications { get; set; } = new();
    public string RestPeriod { get; set; } = string.Empty;
    public string RPETarget { get; set; } = string.Empty; // Rating of Perceived Exertion
}

public class ExerciseParameters
{
    public string Sets { get; set; } = string.Empty;
    public string Repetitions { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public string Tempo { get; set; } = string.Empty; // e.g., "3-1-1-1"
    public string RangeOfMotion { get; set; } = string.Empty;
    public bool IsUnilateral { get; set; }
    public string ProgressionMethod { get; set; } = string.Empty;
}

public class CooldownSection
{
    public TimeSpan Duration { get; set; }
    public List<CooldownExercise> Exercises { get; set; } = new();
    public string Purpose { get; set; } = string.Empty;
    public List<string> RecoveryTips { get; set; } = new();
}

public class CooldownExercise
{
    public string Name { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public List<string> MuscleGroups { get; set; } = new();
    public bool IsStretching { get; set; }
    public string Breathing { get; set; } = string.Empty;
}

public class NutritionalGuidance
{
    public string PreWorkout { get; set; } = string.Empty;
    public string PostWorkout { get; set; } = string.Empty;
    public string Hydration { get; set; } = string.Empty;
    public List<string> GeneralTips { get; set; } = new();
}

public class SafetyNote
{
    public string Note { get; set; } = string.Empty;
    public SafetyLevel Level { get; set; }
    public List<string> RelatedExercises { get; set; } = new();
    public string Action { get; set; } = string.Empty;
}

public class ExerciseInstruction
{
    public string ExerciseName { get; set; } = string.Empty;
    public List<string> StepByStep { get; set; } = new();
    public List<string> KeyPoints { get; set; } = new();
    public List<string> CommonErrors { get; set; } = new();
    public string TargetMuscles { get; set; } = string.Empty;
    public int QualityScore { get; set; } // 1-10 scale
}

public class SpanishValidationResult
{
    public bool IsValid { get; set; }
    public double LanguageQualityScore { get; set; } // 0.0 - 1.0
    public List<LanguageError> Errors { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public bool HasProperFitnessTerminology { get; set; }
    public bool HasCorrectGrammar { get; set; }
    public bool HasAppropriateFormality { get; set; }
    public int SpellingErrors { get; set; }
    public int GrammarErrors { get; set; }
    public List<string> ImprovedSentences { get; set; } = new();
}

public class LanguageError
{
    public string ErrorType { get; set; } = string.Empty; // "Spelling", "Grammar", "Terminology", "Formality"
    public string OriginalText { get; set; } = string.Empty;
    public string SuggestedCorrection { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public int Position { get; set; }
    public ErrorSeverity Severity { get; set; }
}

public class RoutineQualityScore
{
    public double OverallScore { get; set; } // 0.0 - 10.0
    public QualityMetrics Metrics { get; set; } = new();
    public List<QualityInsight> Insights { get; set; } = new();
    public List<string> StrengthAreas { get; set; } = new();
    public List<string> ImprovementAreas { get; set; } = new();
    public bool MeetsQualityThreshold { get; set; }
}

public class QualityMetrics
{
    public double CompletenessScore { get; set; } // Has all sections
    public double ClarityScore { get; set; } // Instructions are clear
    public double SafetyScore { get; set; } // Safety considerations included
    public double PersonalizationScore { get; set; } // Matches user parameters
    public double ScientificAccuracyScore { get; set; } // Exercise science principles
    public double ProgressionScore { get; set; } // Logical progression plan
    public double PracticalityScore { get; set; } // Realistic and achievable
}

public class QualityInsight
{
    public string Category { get; set; } = string.Empty;
    public string Observation { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public InsightPriority Priority { get; set; }
}

public class ProcessingWarning
{
    public string WarningType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string SuggestedAction { get; set; } = string.Empty;
    public WarningSeverity Severity { get; set; }
    public bool RequiresUserAttention { get; set; }
}

// Enums
public enum ErrorSeverity
{
    Minor = 1,
    Moderate = 2,
    Major = 3,
    Critical = 4
}

public enum InsightPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum WarningSeverity
{
    Info = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}