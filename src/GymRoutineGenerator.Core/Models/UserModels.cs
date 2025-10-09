using GymRoutineGenerator.Core.Enums;

namespace GymRoutineGenerator.Core.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Alias for Id, for compatibility
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string FitnessLevel { get; set; } = string.Empty;
        public int TrainingDays { get; set; }
        public int TrainingDaysPerWeek { get; set; } // Agregado para compatibilidad
        public List<string> Goals { get; set; } = new();
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // Agregado para compatibilidad
        public bool IsActive { get; set; } = true;

        // Additional properties for Infrastructure compatibility
        public List<string> PhysicalLimitations { get; set; } = new();
        public string ExperienceLevel { get; set; } = string.Empty;
        public List<string> InjuryHistory { get; set; } = new();
    }

    public class UserRoutine
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Exercise> Exercises { get; set; } = new();
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public string Difficulty { get; set; } = string.Empty;
        public int EstimatedDuration { get; set; }
        public string RoutineType { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string RoutineData { get; set; } = string.Empty; // JSON serialized version for compatibility
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // For compatibility with Infrastructure layer

        // Navigation properties for EF compatibility
        public List<ExerciseModification> RoutineExercises { get; set; } = new(); // Alias for exercises
        public List<ExerciseModification> Modifications { get; set; } = new();
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow; // Alias for LastModified
        public string UserName { get; set; } = string.Empty; // For compatibility
        public List<string> Goals { get; set; } = new(); // For compatibility

        // Additional compatibility properties
        public int Rating { get; set; } = 0; // For SmartPromptService
        public RoutineStatus Status { get; set; } = RoutineStatus.Active; // Enum for status
        public bool IsFavorite { get; set; } = false;
        public string UserIdString { get; set; } = string.Empty; // String version for compatibility
        public string RoutineContent { get; set; } = string.Empty; // JSON content
        public int Age { get; set; } = 0; // User age for routine context
        public string Gender { get; set; } = string.Empty; // User gender
        public string FitnessLevel { get; set; } = string.Empty; // User fitness level
        public int TrainingDays { get; set; } = 0; // Training days per week
    }

    // REMOVED: UserPhysicalLimitation - Use GymRoutineGenerator.Data.Entities.UserPhysicalLimitation instead

    public class ExerciseModification
    {
        public int Id { get; set; }
        public int? ExerciseId { get; set; } // Nullable to allow modifications without specific exercise
        public Exercise? Exercise { get; set; } // Navigation property
        public string ModificationType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Justification { get; set; } = string.Empty;
        public int? ReplacementExerciseId { get; set; }
        public int? NewExerciseId { get; set; } // Alias for ReplacementExerciseId
        public Exercise? ReplacementExercise { get; set; } // Navigation property
        public Dictionary<string, object> Parameters { get; set; } = new();
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool RequiresUserConfirmation { get; set; } = false;
        public string OriginalValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;

        // Additional properties for RoutineModificationService
        public int? RoutineId { get; set; }
        public string UserMessage { get; set; } = string.Empty;
        public List<SafetyWarning> SafetyWarnings { get; set; } = new();
    }

    public class WorkoutSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoutineId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Notes { get; set; } = string.Empty;
        public Dictionary<string, object> Metrics { get; set; } = new();
        public List<ExercisePerformance> ExercisePerformances { get; set; } = new();

        // Additional properties for compatibility with IntelligentExportService
        public bool Completed { get; set; } = false;
        public double CaloriesBurned { get; set; } = 0;
        public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : TimeSpan.Zero;
        public string Location { get; set; } = string.Empty;
        public int? OverallRPE { get; set; } // Rate of Perceived Exertion (1-10)
        public int? PerceivedExertion { get; set; } // Alias for OverallRPE
        public string SpecialNotes { get; set; } = string.Empty; // Additional notes field

        // Additional properties for RoutineStructureService compatibility
        public List<string> TargetMuscleGroups { get; set; } = new();
        public int EstimatedDuration { get; set; } = 0; // in minutes
        public string TargetIntensity { get; set; } = string.Empty;
        public List<Exercise> Warmup { get; set; } = new(); // Warmup exercises
        public List<Exercise> Cooldown { get; set; } = new(); // Cooldown exercises
        public List<Exercise> Exercises { get; set; } = new(); // Main exercises
        public List<string> IssuesEncountered { get; set; } = new();
        public float Quality { get; set; } = 0.0f;
    }

    public class ExercisePerformance
    {
        public int Id { get; set; }
        public int WorkoutSessionId { get; set; }
        public int ExerciseId { get; set; }
        public int Sets { get; set; }
        public string Reps { get; set; } = string.Empty;
        public string Weight { get; set; } = string.Empty;
        public int? RPE { get; set; }
        public string Notes { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }

        // Additional properties for compatibility
        public string ExerciseName { get; set; } = string.Empty;
        public double AverageRPE => RPE ?? 0; // Alias/computed property
    }

    public class TimeRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Static factory methods for common time ranges
        public static TimeRange LastWeek => new TimeRange
        {
            StartDate = DateTime.Now.AddDays(-7),
            EndDate = DateTime.Now
        };

        public static TimeRange LastMonth => new TimeRange
        {
            StartDate = DateTime.Now.AddMonths(-1),
            EndDate = DateTime.Now
        };

        public static TimeRange LastQuarter => new TimeRange
        {
            StartDate = DateTime.Now.AddMonths(-3),
            EndDate = DateTime.Now
        };

        public static TimeRange LastYear => new TimeRange
        {
            StartDate = DateTime.Now.AddYears(-1),
            EndDate = DateTime.Now
        };
    }

}