using GymRoutineGenerator.Core.Enums;

namespace GymRoutineGenerator.Core.Models
{
    public class ExerciseClassificationResult
    {
        public string ExerciseName { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public List<string> AlternativeNames { get; set; } = new();
        public string MuscleGroup { get; set; } = string.Empty;
        public DifficultyLevel EstimatedDifficulty { get; set; }
        public List<string> DetectedEquipment { get; set; } = new();
        public List<string> FormNotes { get; set; } = new();
        public bool IsValidExercise { get; set; }
        public string ClassificationMethod { get; set; } = string.Empty;
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

        // Additional properties for Infrastructure compatibility
        public string Category { get; set; } = string.Empty;
        public float CategoryConfidence { get; set; } = 0.0f;
        public List<string> RequiredSkills { get; set; } = new();
        public List<string> SafetyConsiderations { get; set; } = new();
        public bool RequiresSpotter { get; set; } = false;
        public string RecommendedEnvironment { get; set; } = string.Empty;
    }

    public class ImageAnalysisResult
    {
        public string Description { get; set; } = string.Empty;
        public List<DetectedObject> DetectedObjects { get; set; } = new();
        public List<string> DetectedActions { get; set; } = new();
        public ImageQuality Quality { get; set; } = new();
        public List<string> SafetyObservations { get; set; } = new();
        public float AnalysisConfidence { get; set; }
        public string AnalysisMethod { get; set; } = string.Empty;
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> RawData { get; set; } = new();
        public PersonDetection? PersonDetection { get; set; }
        public EquipmentDetection? EquipmentDetection { get; set; }
        public MovementAnalysis? MovementAnalysis { get; set; }
        public EnvironmentAnalysis? EnvironmentAnalysis { get; set; }
        public List<string> ProcessingNotes { get; set; } = new();
    }

    public class DetectedObject
    {
        public string ObjectType { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public BoundingBox BoundingBox { get; set; } = new();
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    public class BoundingBox
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class ImageQuality
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Format { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public float Sharpness { get; set; }
        public float Clarity { get; set; } // Alias for Sharpness
        public float Brightness { get; set; }
        public float Lighting { get; set; } // Alias for Brightness
        public float Contrast { get; set; }
        public float Composition { get; set; }
        public bool IsBlurry { get; set; }
        public bool IsOverexposed { get; set; }
        public bool IsUnderexposed { get; set; }
        public string QualityRating { get; set; } = string.Empty; // "Poor", "Fair", "Good", "Excellent"
        public bool IsPersonVisible { get; set; } = false;
        public bool IsEquipmentVisible { get; set; } = false;
        public float OverallScore { get; set; } = 0.0f;
        public List<string> QualityIssues { get; set; } = new();
    }

    public class PostureAnalysis
    {
        public List<string> DetectedPostureIssues { get; set; } = new();
        public List<string> FormRecommendations { get; set; } = new();
        public float PostureScore { get; set; } // 0-100
        public Dictionary<string, float> JointAngles { get; set; } = new();
        public List<string> SafetyWarnings { get; set; } = new();
        public bool IsProperForm { get; set; }
    }

    public class MovementPattern
    {
        public string PatternType { get; set; } = string.Empty;
        public string Phase { get; set; } = string.Empty; // "Concentric", "Eccentric", "Isometric"
        public float MovementQuality { get; set; }
        public List<string> Observations { get; set; } = new();
        public Dictionary<string, object> BiomechanicalData { get; set; } = new();
    }

    public class BodyPositionAnalysis
    {
        public string ExerciseName { get; set; } = string.Empty;
        public PostureAnalysis PostureAnalysis { get; set; } = new();
        public List<MovementPattern> MovementPatterns { get; set; } = new();
        public float OverallFormScore { get; set; } // 0-100
        public List<string> CriticalIssues { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public bool RequiresCorrection { get; set; }
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
        public string PrimaryPosition { get; set; } = string.Empty;
        public Dictionary<string, float> JointPositions { get; set; } = new();
        public SpinalAlignment? SpinalAlignment { get; set; }
        public WeightDistribution? WeightDistribution { get; set; }
        public string FormAssessment { get; set; } = string.Empty;
    }

    public enum EnvironmentType
    {
        Indoor,
        Outdoor,
        Home,
        Gym,
        Studio,
        Unknown
    }

    public class PersonDetection
    {
        public string PersonId { get; set; } = string.Empty;
        public BoundingBox BoundingBox { get; set; } = new();
        public float Confidence { get; set; }
        public List<JointPosition> Joints { get; set; } = new();
        public Dictionary<string, object> Attributes { get; set; } = new();
        public bool IsPersonDetected { get; set; } = false;
        public int NumberOfPeople { get; set; } = 0;
        public List<string> DetectedBodyParts { get; set; } = new();
        public string EstimatedGender { get; set; } = string.Empty;
        public string EstimatedFitnessLevel { get; set; } = string.Empty;
    }

    public class EquipmentDetection
    {
        public string EquipmentType { get; set; } = string.Empty;
        public BoundingBox BoundingBox { get; set; } = new();
        public float Confidence { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
        public List<string> Equipment { get; set; } = new();
        public string EnvironmentType { get; set; } = string.Empty;
        public bool IsHomeGym { get; set; } = false;
        public bool IsCommercialGym { get; set; } = false;
    }

    public class MovementAnalysis
    {
        public string MovementType { get; set; } = string.Empty;
        public List<MovementPhase> Phases { get; set; } = new();
        public float Quality { get; set; }
        public List<string> Issues { get; set; } = new();
        public string Phase { get; set; } = string.Empty;
        public string PrimaryPlane { get; set; } = string.Empty;
        public List<string> SecondaryPlanes { get; set; } = new();
        public bool IsStaticHold { get; set; }
        public bool IsDynamicMovement { get; set; }
        public float EstimatedIntensity { get; set; }
    }

    public class MovementPhase
    {
        public string PhaseName { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public float Quality { get; set; }
        public List<string> Observations { get; set; } = new();
    }

    public class EnvironmentAnalysis
    {
        public EnvironmentType EnvironmentType { get; set; }
        public List<string> DetectedEquipment { get; set; } = new();
        public List<string> SafetyHazards { get; set; } = new();
        public Dictionary<string, object> EnvironmentFactors { get; set; } = new();
        public string Location { get; set; } = string.Empty;
        public List<string> VisibleEquipment { get; set; } = new();
        public List<string> SafetyFeatures { get; set; } = new();
        public string SpaceAvailability { get; set; } = string.Empty;
        public string Lighting { get; set; } = string.Empty;
    }

    public class JointPosition
    {
        public string JointName { get; set; } = string.Empty;
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Confidence { get; set; }
        public bool IsVisible { get; set; }
        public float Angle { get; set; }
        public string Position { get; set; } = string.Empty;
        public bool IsOptimal { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class FormFeedback
    {
        public string ExerciseName { get; set; } = string.Empty;
        public float OverallScore { get; set; }
        public List<string> PositiveFeedback { get; set; } = new();
        public List<string> ImprovementAreas { get; set; } = new();
        public List<string> TechniqueTips { get; set; } = new();
        public List<string> SafetyWarnings { get; set; } = new();
    }

    public class ComparisonResult
    {
        public float SimilarityScore { get; set; }
        public List<string> Differences { get; set; } = new();
        public List<string> Improvements { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    public class SpinalAlignment
    {
        public float SpinalCurvature { get; set; }
        public bool IsNeutral { get; set; }
        public List<string> Issues { get; set; } = new();
        public float AlignmentScore { get; set; }
        public string CurvatureAssessment { get; set; } = string.Empty;
    }

    public class WeightDistribution
    {
        public float LeftPercentage { get; set; }
        public float RightPercentage { get; set; }
        public bool IsBalanced { get; set; }
        public float DistributionScore { get; set; }
        public List<string> Recommendations { get; set; } = new();
        public string Distribution { get; set; } = string.Empty;
        public float BalanceScore { get; set; }
    }

    public class ExerciseClassification
    {
        public string ExerciseName { get; set; } = string.Empty;
        public string MuscleGroup { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public bool IsValidExercise { get; set; }
        public string EstimatedDifficulty { get; set; } = string.Empty;
        public List<string> DetectedEquipment { get; set; } = new();
    }

    public class ImageQualityAssessment
    {
        public string QualityRating { get; set; } = string.Empty;
        public bool IsBlurry { get; set; }
        public bool HasProperLighting { get; set; }
        public bool IsPersonVisible { get; set; }
        public float OverallScore { get; set; }
        public List<string> Issues { get; set; } = new();
    }

    public class BodyPart
    {
        public string Name { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public bool IsVisible { get; set; }
    }

    public class DetectedEquipment
    {
        public string Name { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public EquipmentCategory Category { get; set; }
        public ConditionAssessment Condition { get; set; } = new();
    }

    public class ConditionAssessment
    {
        public EquipmentCondition Condition { get; set; }
        public float SafetyScore { get; set; }
        public bool RecommendedForUse { get; set; }
    }

    public class SafetyFeatures
    {
        public bool HasSafetyRacks { get; set; }
        public bool HasFirstAid { get; set; }
        public List<string> AdditionalFeatures { get; set; } = new();
    }

    public enum EquipmentCategory
    {
        FreeWeights,
        Machines,
        Cardio,
        Bodyweight,
        Accessories,
        Other
    }

    public enum EquipmentCondition
    {
        Excellent,
        Good,
        Fair,
        Poor,
        Unsafe
    }

    public enum GymEnvironmentType
    {
        Home,
        Commercial,
        Outdoor,
        Studio,
        Unknown
    }

    public enum LocationType
    {
        IndoorGym,
        OutdoorArea,
        Home,
        Studio,
        Unknown
    }

    public enum LightingConditions
    {
        Excellent,
        Good,
        Fair,
        Poor,
        Dark
    }

    public enum MovementPlane
    {
        Sagittal,
        Frontal,
        Transverse,
        Multiple
    }
}