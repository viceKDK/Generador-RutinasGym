using System.ComponentModel.DataAnnotations;

namespace GymRoutineGenerator.Data.Entities;

public class UserProfile
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(15, 100)]
    public int Age { get; set; }

    [Required]
    [MaxLength(20)]
    public string Gender { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string FitnessLevel { get; set; } = "Principiante";

    public int TrainingDaysPerWeek { get; set; } = 3;
    public int TrainingDays { get; set; } = 3; // Alias for compatibility

    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    // Additional properties for compatibility with Core layer
    public List<string> Goals { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public List<string> PhysicalLimitationsList { get; set; } = new(); // For compatibility
    public string ExperienceLevel { get; set; } = string.Empty;
    public List<string> InjuryHistory { get; set; } = new();

    // Navegación
    public virtual List<UserRoutine> RoutineHistory { get; set; } = new();
    public virtual List<UserEquipmentPreference> EquipmentPreferences { get; set; } = new();
    public virtual List<UserMuscleGroupPreference> MuscleGroupPreferences { get; set; } = new();
    public virtual List<UserPhysicalLimitation> PhysicalLimitations { get; set; } = new();
}

public class UserRoutine
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    [Required]
    public string RoutineData { get; set; } = string.Empty; // JSON serialized

    [MaxLength(20)]
    public string Status { get; set; } = "ACTIVE";

    [Range(1, 5)]
    public int? Rating { get; set; }

    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;

    // Additional properties for compatibility with Core layer
    public string Description { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int EstimatedDuration { get; set; } = 0;
    public string RoutineType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsFavorite { get; set; } = false;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string UserName { get; set; } = string.Empty;
    public List<string> Goals { get; set; } = new();
    public string UserIdString { get; set; } = string.Empty;
    public string RoutineContent { get; set; } = string.Empty;
    public int Age { get; set; } = 0;
    public string Gender { get; set; } = string.Empty;
    public string FitnessLevel { get; set; } = string.Empty;
    public int TrainingDays { get; set; } = 0;
    public int TrainingDaysPerWeek { get; set; } = 0;

    // Navegación
    public virtual UserProfile User { get; set; } = null!;
    public virtual List<RoutineModification> Modifications { get; set; } = new();
    public virtual List<RoutineExercise> RoutineExercises { get; set; } = new();
}

public class UserEquipmentPreference
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public int UserProfileId { get; set; } // Agregado para compatibilidad

    [Required]
    [MaxLength(100)]
    public string EquipmentType { get; set; } = string.Empty;
    public int EquipmentTypeId { get; set; } // Agregado para compatibilidad

    public bool IsAvailable { get; set; } = true;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    // Navegación
    public virtual UserProfile User { get; set; } = null!;
    public virtual EquipmentType? EquipmentTypeEntity { get; set; }
}

public class UserMuscleGroupPreference
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public int UserProfileId { get; set; } // Agregado para compatibilidad

    public int MuscleGroupId { get; set; }

    [Range(1, 5)]
    public int Priority { get; set; } = 3; // 1 = highest priority

    public bool IsRestricted { get; set; } = false;
    public EmphasisLevel EmphasisLevel { get; set; } = EmphasisLevel.Medium; // Agregado para compatibilidad

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    // Navegación
    public virtual UserProfile User { get; set; } = null!;
    public virtual MuscleGroup MuscleGroup { get; set; } = null!;
}

public class UserPhysicalLimitation
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public int UserProfileId { get; set; } // Agregado para compatibilidad

    [Required]
    [MaxLength(100)]
    public string LimitationType { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Severity { get; set; } = 3; // 1 = mild, 5 = severe
    public string SeverityLevel { get; set; } = "Medium"; // String version for compatibility

    public string ExercisesToAvoid { get; set; } = "[]"; // JSON array
    public List<string> ExercisesToAvoidList { get; set; } = new(); // List version for compatibility
    public List<string> CustomRestrictions { get; set; } = new(); // Agregado para compatibilidad
    public List<string> AffectedBodyParts { get; set; } = new(); // For Core.Models compatibility
    public List<string> RestrictedMovements { get; set; } = new(); // For Core.Models compatibility
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Agregado para compatibilidad
    public DateTime DateReported { get; set; } = DateTime.UtcNow; // For Core.Models compatibility
    public bool IsActive { get; set; } = true; // For Core.Models compatibility

    // Navegación
    public virtual UserProfile User { get; set; } = null!;
}

public class RoutineModification
{
    [Key]
    public int Id { get; set; }

    public int UserRoutineId { get; set; }

    public DateTime ModifiedAt { get; set; }

    [Required]
    [MaxLength(100)]
    public string ModificationType { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string OriginalValue { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string NewValue { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string UserRequest { get; set; } = string.Empty; // Original user request for the modification

    [MaxLength(10)]
    public string ModifiedBy { get; set; } = "USER"; // "USER" o "AI"

    public bool WasApplied { get; set; } = false; // For Infrastructure compatibility

    // Navegación
    public virtual UserRoutine UserRoutine { get; set; } = null!;
}

public enum RoutineStatus
{
    ACTIVE,
    COMPLETED,
    MODIFIED,
    ARCHIVED,
    FAVORITE
}

public enum Gender
{
    Male,
    Female,
    Other,
    PreferNotToSay,
    // Agregados para compatibilidad
    Hombre = Male,
    Mujer = Female,
    Otro = Other
}

public enum LimitationType
{
    Knee,
    Back,
    Shoulder,
    Wrist,
    Ankle,
    Neck,
    Hip,
    Other,
    // Agregados para compatibilidad con UserParameterMappingService
    ProblemasEspalda,
    ProblemasRodilla,
    ProblemasHombro,
    ProblemasCardivasculares,
    Artritis,
    Personalizada,
    // Agregados adicionales para los servicios
    LesionReciente,
    Embarazo,
    CondicionPreexistente,
    Recuperacion
}

public enum EmphasisLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    // Agregados para compatibilidad
    Bajo = 1,
    Medio = 2,
    Alto = 3
}