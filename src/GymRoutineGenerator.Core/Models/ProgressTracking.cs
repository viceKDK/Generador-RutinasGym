using System;
using System.Collections.Generic;

namespace GymRoutineGenerator.Core.Models
{
    public class ProgressMetric
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string MetricType { get; set; } = string.Empty; // Weight, BodyFat, Strength, Endurance, etc.
        public string ExerciseName { get; set; } = string.Empty; // For exercise-specific metrics
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty; // kg, lbs, %, seconds, etc.
        public DateTime RecordedDate { get; set; } = DateTime.UtcNow;
        public string Notes { get; set; } = string.Empty;
        public bool IsPersonalRecord { get; set; }
        public string MeasurementContext { get; set; } = string.Empty; // Training, Competition, Assessment
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    public class DetailedWorkoutSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int UserRoutineId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public string Location { get; set; } = string.Empty;
        public double OverallRPE { get; set; } // Rate of Perceived Exertion
        public string Notes { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public List<DetailedExercisePerformance> ExercisePerformances { get; set; } = new();
        public List<EnvironmentalFactor> EnvironmentalFactors { get; set; } = new();
        public double CaloriesBurned { get; set; }
        public int HeartRateAverage { get; set; }
        public int HeartRateMax { get; set; }
        public string MoodBefore { get; set; } = string.Empty;
        public string MoodAfter { get; set; } = string.Empty;
        public int EnergyLevelBefore { get; set; } // 1-10 scale
        public int EnergyLevelAfter { get; set; } // 1-10 scale
    }

    public class DetailedExercisePerformance
    {
        public int Id { get; set; }
        public int WorkoutSessionId { get; set; }
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public List<SetPerformance> Sets { get; set; } = new();
        public double RPE { get; set; }
        public string Notes { get; set; } = string.Empty;
        public TimeSpan RestTime { get; set; }
        public bool PersonalRecord { get; set; }
        public double TotalVolume => Sets.Sum(s => s.Weight * s.ActualReps);
        public double AverageWeight => Sets.Any() ? Sets.Average(s => s.Weight) : 0;
        public int TotalReps => Sets.Sum(s => s.ActualReps);
    }

    public class SetPerformance
    {
        public int SetNumber { get; set; }
        public int PlannedReps { get; set; }
        public int ActualReps { get; set; }
        public int RepsCompleted => ActualReps; // Alias
        public double PlannedWeight { get; set; }
        public double Weight { get; set; }
        public double WeightUsed => Weight; // Alias
        public TimeSpan Duration { get; set; }
        public double RPE { get; set; }
        public bool Completed { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class EnvironmentalFactor
    {
        public string FactorType { get; set; } = string.Empty; // Temperature, Humidity, Equipment, Crowding, etc.
        public string Value { get; set; } = string.Empty;
        public int Impact { get; set; } // -5 to +5 scale
        public string Notes { get; set; } = string.Empty;
    }


    public class OverallProgress
    {
        public ProgressTrend StrengthTrend { get; set; }
        public ProgressTrend EnduranceTrend { get; set; }
        public ProgressTrend ConsistencyTrend { get; set; }
        public ProgressTrend MotivationTrend { get; set; }
        public double OverallScore { get; set; } // 0-100
        public string Summary { get; set; } = string.Empty;
        public List<string> KeyWins { get; set; } = new();
        public List<string> AreasForImprovement { get; set; } = new();
    }

    public class ExerciseProgress
    {
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public ProgressTrend VolumeProgress { get; set; }
        public ProgressTrend IntensityProgress { get; set; }
        public ProgressTrend TechniqueProgress { get; set; }
        public double StartingWeight { get; set; }
        public double CurrentWeight { get; set; }
        public double WeightIncrease => CurrentWeight - StartingWeight;
        public double PercentageIncrease => StartingWeight > 0 ? (WeightIncrease / StartingWeight) * 100 : 0;
        public int SessionsPerformed { get; set; }
        public DateTime LastPerformed { get; set; }
        public List<Milestone> Milestones { get; set; } = new();
        public string ProgressNotes { get; set; } = string.Empty;
    }

    public class ProgressConcern
    {
        public string Area { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public SeverityLevel Severity { get; set; }
        public List<string> PossibleCauses { get; set; } = new();
        public List<string> RecommendedActions { get; set; } = new();
        public bool RequiresProfessionalConsultation { get; set; }
        public string Timeline { get; set; } = string.Empty;
    }

    public class Achievement
    {
        public string Type { get; set; } = string.Empty; // PR, Consistency, Technique, etc.
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateAchieved { get; set; }
        public string Value { get; set; } = string.Empty; // Specific achievement value
        public ImportanceLevel Importance { get; set; }
        public bool IsMajorMilestone { get; set; }
        public string MotivationalMessage { get; set; } = string.Empty;
    }

    public class ProgressionRecommendations
    {
        public List<TrainingAdjustment> TrainingAdjustments { get; set; } = new();
        public List<NutritionRecommendation> NutritionRecommendations { get; set; } = new();
        public List<RecoveryRecommendation> RecoveryRecommendations { get; set; } = new();
        public List<TechniqueImprovement> TechniqueImprovements { get; set; } = new();
        public string NextPhaseRecommendation { get; set; } = string.Empty;
        public DateTime RecommendedReviewDate { get; set; }
    }

    public class TrainingAdjustment
    {
        public string Type { get; set; } = string.Empty; // Volume, Intensity, Frequency, etc.
        public string Description { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public ProgressTimeFrame Implementation { get; set; }
        public List<string> SpecificChanges { get; set; } = new();
        public string ExpectedOutcome { get; set; } = string.Empty;
    }

    public class NutritionRecommendation
    {
        public string Area { get; set; } = string.Empty; // Pre-workout, Post-workout, General, etc.
        public string Recommendation { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
        public bool IsEssential { get; set; }
        public List<string> SpecificFoods { get; set; } = new();
        public string Timing { get; set; } = string.Empty;
    }

    public class RecoveryRecommendation
    {
        public string Type { get; set; } = string.Empty; // Sleep, Active Recovery, Rest Days, etc.
        public string Recommendation { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public List<string> SpecificActions { get; set; } = new();
        public string ExpectedBenefit { get; set; } = string.Empty;
    }

    public class TechniqueImprovement
    {
        public string ExerciseName { get; set; } = string.Empty;
        public string IssueIdentified { get; set; } = string.Empty;
        public string Improvement { get; set; } = string.Empty;
        public List<string> PracticeSteps { get; set; } = new();
        public string VideoReference { get; set; } = string.Empty;
        public bool RequiresCoaching { get; set; }
    }

    public class Milestone
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateAchieved { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public bool IsPersonalRecord { get; set; }
    }

    public class ProgressTimeRange
    {
        public DateTime StartDate { get; set; } = DateTime.UtcNow.AddDays(-30);
        public DateTime EndDate { get; set; } = DateTime.UtcNow;
        public int Days => (EndDate - StartDate).Days;
        public int Weeks => Days / 7;
        public int Months => (int)((EndDate.Year - StartDate.Year) * 12 + EndDate.Month - StartDate.Month);

        public static ProgressTimeRange LastWeek => new ProgressTimeRange
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow
        };

        public static ProgressTimeRange LastMonth => new ProgressTimeRange
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow
        };

        public static ProgressTimeRange LastQuarter => new ProgressTimeRange
        {
            StartDate = DateTime.UtcNow.AddDays(-90),
            EndDate = DateTime.UtcNow
        };

        public static ProgressTimeRange LastYear => new ProgressTimeRange
        {
            StartDate = DateTime.UtcNow.AddDays(-365),
            EndDate = DateTime.UtcNow
        };
    }

    public enum ProgressTrend
    {
        Declining,
        Stagnant,
        SlowGrowth,
        SteadyGrowth,
        RapidGrowth,
        Inconsistent,
        Unknown
    }

    // Priority enum is already defined in ServiceModels.cs

    public enum ProgressTimeFrame
    {
        Immediate,    // Within 1 week
        Short,        // 1-4 weeks
        Medium,       // 1-3 months
        Long,         // 3-6 months
        Extended      // 6+ months
    }

    public enum SeverityLevel
    {
        Minor,
        Moderate,
        Significant,
        Critical
    }

    public enum ImportanceLevel
    {
        Minor,
        Standard,
        Major,
        Milestone
    }
}