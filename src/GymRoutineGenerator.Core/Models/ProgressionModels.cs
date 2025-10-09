namespace GymRoutineGenerator.Core.Models
{

    public class Goal
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public GoalType Type { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public double StartValue { get; set; }
        public double TargetValue { get; set; }
        public double CurrentValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }
        public GoalStatus Status { get; set; }
        public float CompletionPercentage { get; set; }
        public List<string> Milestones { get; set; } = new();
        public Dictionary<string, object> CustomCriteria { get; set; } = new();
    }

    public class PerformanceInsight
    {
        public string InsightType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public InsightPriority Priority { get; set; }
        public List<string> ActionItems { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> SupportingData { get; set; } = new();
    }

    public enum ProgressionTrend
    {
        Improving,
        Stable,
        Declining,
        Plateaued,
        Inconsistent
    }

    public enum AchievementType
    {
        Milestone,
        Streak,
        Personal_Record,
        Consistency,
        Technique,
        Endurance,
        Strength,
        Special_Event
    }

    public enum AchievementCategory
    {
        Strength,
        Endurance,
        Flexibility,
        Consistency,
        Form,
        Progression,
        Social,
        Special
    }

    public enum AchievementRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum GoalType
    {
        Strength,
        Endurance,
        Weight_Loss,
        Weight_Gain,
        Skill_Development,
        Consistency,
        Performance,
        Health_Metric
    }

    public enum GoalStatus
    {
        Active,
        Completed,
        Paused,
        Cancelled,
        Overdue
    }

    public enum InsightPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class ProgressionAnalysis
    {
        public int UserId { get; set; }
        public DateTime AnalysisDate { get; set; }
        public TimeSpan TrainingDuration { get; set; }
        public TimeSpan AnalysisPeriod { get; set; } = TimeSpan.FromDays(30); // Default 30 days
        public ProgressionPhase CurrentPhase { get; set; }
        public OverallProgressAssessment OverallProgress { get; set; } = new();
        public List<ExerciseProgressAnalysis> ExerciseProgress { get; set; } = new();
        public List<MuscleGroupProgress> MuscleGroupProgress { get; set; } = new();
        public PerformanceMetrics Performance { get; set; } = new();
        public List<ProgressionOpportunity> Opportunities { get; set; } = new();
        public List<string> Achievements { get; set; } = new();
        public List<Achievement> AchievementsList { get; set; } = new(); // For type compatibility
        public List<string> AreasForImprovement { get; set; } = new();
        public float ProgressScore { get; set; }

        // Additional properties for compatibility
        public List<string> AIInsights { get; set; } = new();
    }

    public enum ProgressionPhase
    {
        Beginner_Gains,
        Intermediate_Development,
        Advanced_Refinement,
        Expert_Maintenance
    }

    public class OverallProgressAssessment
    {
        public ProgressionTrend StrengthTrend { get; set; }
        public ProgressionTrend EnduranceTrend { get; set; }
        public ProgressionTrend ConsistencyTrend { get; set; }
        public ProgressionTrend TechniqueTrend { get; set; }
        public float OverallScore { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<string> KeyInsights { get; set; } = new();
    }

    public class ExerciseProgressAnalysis
    {
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public List<DataPoint> PerformanceHistory { get; set; } = new();
        public ProgressionTrend WeightProgression { get; set; }
        public ProgressionTrend RepsProgression { get; set; }
        public ProgressionTrend VolumeProgression { get; set; }
        public int SessionsCompleted { get; set; }
        public DateTime LastPerformed { get; set; }
        public List<string> RecommendedActions { get; set; } = new();
        public string NextStep { get; set; } = string.Empty;
    }

    public class DataPoint
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string MetricType { get; set; } = string.Empty;
    }

    public class MuscleGroupProgress
    {
        public string MuscleGroup { get; set; } = string.Empty;
        public float StrengthGain { get; set; }
        public float VolumeIncrease { get; set; }
        public int ExercisesPerformed { get; set; }
        public ProgressionTrend Trend { get; set; }
        public bool IsBalanced { get; set; }
        public List<string> Recommendations { get; set; } = new();
    }

    public class PerformanceMetrics
    {
        public double TotalVolumeLifted { get; set; }
        public double AverageIntensity { get; set; }
        public int TotalWorkouts { get; set; }
        public float WorkoutConsistency { get; set; }
        public TimeSpan AverageWorkoutDuration { get; set; }
        public float InjuryRate { get; set; }
        public Dictionary<string, int> ExerciseFrequency { get; set; } = new();
        public List<PersonalRecord> PersonalRecords { get; set; } = new();
    }

    public class PersonalRecord
    {
        public string ExerciseName { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime AchievedDate { get; set; }
        public string RecordType { get; set; } = string.Empty;
    }

    public class ProgressionOpportunity
    {
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public List<string> ActionItems { get; set; } = new();
        public TimeFrame EstimatedTimeframe { get; set; }
        public float PotentialImpact { get; set; }
    }

    public enum Priority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum TimeFrame
    {
        Short_Term,    // 1-4 weeks
        Medium_Term,   // 1-3 months
        Long_Term      // 3+ months
    }

    public class ProgressionRecommendation
    {
        public int UserId { get; set; }
        public DateTime GeneratedAt { get; set; }
        public List<ExerciseRecommendation> ExerciseRecommendations { get; set; } = new();
        public List<RoutineAdjustment> RoutineAdjustments { get; set; } = new();
        public List<string> GeneralAdvice { get; set; } = new();
        public TimeFrame ImplementationTimeframe { get; set; }
        public float ConfidenceScore { get; set; }
        public string Rationale { get; set; } = string.Empty;
    }

    public class ExerciseRecommendation
    {
        public string ExerciseName { get; set; } = string.Empty;
        public string RecommendationType { get; set; } = string.Empty; // "Replace", "Add", "Modify"
        public string Reason { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    public class RoutineAdjustment
    {
        public string AdjustmentType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public bool IsOptional { get; set; }
    }

    public class ExerciseProgression
    {
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public ProgressionType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<ProgressionLevel> Levels { get; set; } = new();
        public int CurrentUserLevel { get; set; }
        public int RecommendedNextLevel { get; set; }
    }

    public enum ProgressionType
    {
        Weight_Increase,
        Rep_Increase,
        Set_Increase,
        Tempo_Adjustment,
        Range_Of_Motion,
        Exercise_Variation,
        Complexity_Increase
    }

    public class ProgressionLevel
    {
        public int Level { get; set; }
        public string Name { get; set; } = string.Empty;
        public GymRoutineGenerator.Core.Enums.DifficultyLevel Difficulty { get; set; }
        public ProgressionParameters Parameters { get; set; } = new();
        public string Description { get; set; } = string.Empty;
        public List<string> Requirements { get; set; } = new();
    }

    public class ProgressionParameters
    {
        public int Sets { get; set; }
        public int MinReps { get; set; }
        public int MaxReps { get; set; }
        public float WeightPercentage { get; set; }
        public int RestSeconds { get; set; }
        public string Tempo { get; set; } = string.Empty;
        public Dictionary<string, object> CustomParameters { get; set; } = new();
    }

    public class WorkoutSessionData
    {
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public float PerceivedExertion { get; set; } // RPE 1-10
        public string Notes { get; set; } = string.Empty;
        public List<string> Issues { get; set; } = new();
        public List<ExercisePerformanceData> Exercises { get; set; } = new();
    }

    public class ExercisePerformanceData
    {
        public int ExerciseId { get; set; }
        public List<SetData> Sets { get; set; } = new();
        public string Notes { get; set; } = string.Empty;
    }

    public class SetData
    {
        public int RepsCompleted { get; set; }
        public double WeightUsed { get; set; }
        public float RPE { get; set; } // Rate of Perceived Exertion
        public string Notes { get; set; } = string.Empty;
    }

    public enum SessionQuality
    {
        Poor,
        Below_Average,
        Average,
        Good,
        Excellent
    }

    public class RoutineOptimization
    {
        public int OriginalRoutineId { get; set; }
        public string OriginalRoutineName { get; set; } = string.Empty;
        public string OriginalRoutine { get; set; } = string.Empty; // JSON representation
        public List<OptimizationSuggestion> Suggestions { get; set; } = new();
        public float OptimizationScore { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<string> ExpectedBenefits { get; set; } = new();
    }

    public class OptimizationSuggestion
    {
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public float ImpactScore { get; set; }
        public List<string> ActionSteps { get; set; } = new();
    }

    public class VolumeAnalysis
    {
        public Dictionary<string, double> MuscleGroupVolumes { get; set; } = new();
        public double TotalVolume { get; set; }
        public float VolumeBalance { get; set; }
        public List<string> Imbalances { get; set; } = new();
        public bool IsOptimal { get; set; }
    }

    public class IntensityAnalysis
    {
        public double AverageIntensity { get; set; }
        public Dictionary<string, double> ExerciseIntensities { get; set; } = new();
        public ProgressionTrend IntensityTrend { get; set; }
        public List<string> Observations { get; set; } = new();
        public bool NeedsAdjustment { get; set; }
    }

    public class RecoveryAnalysis
    {
        public TimeSpan AverageRecoveryTime { get; set; }
        public Dictionary<string, TimeSpan> MuscleGroupRecovery { get; set; } = new();
        public float RecoveryScore { get; set; }
        public List<string> RecoveryIssues { get; set; } = new();
        public bool IsAdequate { get; set; }
    }
}