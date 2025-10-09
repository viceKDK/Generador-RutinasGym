using GymRoutineGenerator.Core.Enums;

namespace GymRoutineGenerator.Core.Models
{
    public class SafetyConsideration
    {
        public string Aspect { get; set; } = string.Empty; // "Technique", "Load", "Range_of_Motion", "Speed"
        public string Consideration { get; set; } = string.Empty;
        public string ImpactOnSafety { get; set; } = string.Empty;
        public List<string> MitigationMeasures { get; set; } = new();
        public SafetyLevel RiskLevel { get; set; }
        public SafetyLevel Severity { get; set; } // Alias for RiskLevel
        public List<string> Precautions { get; set; } = new(); // Alias for MitigationMeasures
        public List<string> WarningSignsToStop { get; set; } = new();
    }

    public class ExerciseAlternative
    {
        public Exercise Exercise { get; set; } = new();
        public string ExerciseName { get; set; } = string.Empty; // Alias for Exercise.Name
        public string DifficultyLevel { get; set; } = string.Empty; // Alias for Exercise.DifficultyLevel
        public string Description { get; set; } = string.Empty; // Alias for Exercise.Description
        public string Reason { get; set; } = string.Empty;
        public float SimilarityScore { get; set; }
        public Dictionary<string, object> Adaptations { get; set; } = new();
        public int Priority { get; set; }
        public string ReasonForSuggestion { get; set; } = string.Empty; // Alias for Reason
        public List<string> Benefits { get; set; } = new();
        public List<string> Considerations { get; set; } = new();
    }

    public class ImageRecognitionResult
    {
        public bool Success { get; set; }
        public List<Exercise> MatchedExercises { get; set; } = new();
        public float ConfidenceScore { get; set; }
        public string AnalysisDetails { get; set; } = string.Empty;
        public List<string> DetectedFeatures { get; set; } = new();
        public string ImageQualityAssessment { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
        public TimeSpan ProcessingTime { get; set; }
        public ImageQuality? Quality { get; set; }
    }

    // REMOVED: UserEquipmentPreference - Use GymRoutineGenerator.Data.Entities.UserEquipmentPreference instead
    // REMOVED: UserMuscleGroupPreference - Use GymRoutineGenerator.Data.Entities.UserMuscleGroupPreference instead

    public enum SafetyLevel
    {
        Safe,
        Low_Risk,
        Moderate_Risk,
        High_Risk,
        Dangerous,
        Contraindicated,
        Warning, // Alias for High_Risk
        Info, // Alias for Safe
        Caution, // Alias for Low_Risk
        Critical // Alias for Dangerous
    }


    public enum ExerciseCategory
    {
        Compound,
        Isolation,
        Cardio,
        Strength,
        Flexibility,
        Balance,
        Functional,
        Core,
        Warmup,
        Cooldown,
        Accessory
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
        public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Details { get; set; } = new();
    }
}