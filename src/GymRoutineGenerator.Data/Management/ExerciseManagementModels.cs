using GymRoutineGenerator.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace GymRoutineGenerator.Data.Management;

public class ExerciseCreateRequest
{
    [Required(ErrorMessage = "El nombre en inglés es requerido")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre en español es requerido")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string SpanishName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es requerida")]
    [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Las instrucciones son requeridas")]
    [StringLength(2000, ErrorMessage = "Las instrucciones no pueden exceder 2000 caracteres")]
    public string Instructions { get; set; } = string.Empty;

    [Required(ErrorMessage = "El grupo muscular principal es requerido")]
    public int PrimaryMuscleGroupId { get; set; }

    [Required(ErrorMessage = "El tipo de equipo es requerido")]
    public int EquipmentTypeId { get; set; }

    public int? ParentExerciseId { get; set; }

    [Required(ErrorMessage = "El nivel de dificultad es requerido")]
    public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Beginner;

    [Required(ErrorMessage = "El tipo de ejercicio es requerido")]
    public ExerciseType ExerciseType { get; set; } = ExerciseType.Strength;

    [Range(1, 3600, ErrorMessage = "La duración debe estar entre 1 y 3600 segundos")]
    public int? DurationSeconds { get; set; }

    public bool IsActive { get; set; } = true;

    public List<int> SecondaryMuscleGroupIds { get; set; } = new();

    public List<ExerciseImageUpload> Images { get; set; } = new();

    public string? Notes { get; set; }

    public string? VideoUrl { get; set; }
}

public class ExerciseUpdateRequest : ExerciseCreateRequest
{
    [Required]
    public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }
}

public class ExerciseImageUpload
{
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Position { get; set; } = "default"; // start, mid, end, demonstration, default
    public bool IsPrimary { get; set; } = false;
    public string Description { get; set; } = string.Empty;
}

public class ExerciseManagementResult
{
    public bool Success { get; set; }
    public int? ExerciseId { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ExerciseDeletionCheck
{
    public bool CanDelete { get; set; }
    public List<string> Dependencies { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public int ChildExercisesCount { get; set; }
    public int RoutineUsageCount { get; set; }
    public bool HasImages { get; set; }
}

public class BulkExerciseOperation
{
    public BulkOperationType Operation { get; set; }
    public List<int> ExerciseIds { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class BulkOperationResult
{
    public bool Success { get; set; }
    public int TotalItems { get; set; }
    public int SuccessfulItems { get; set; }
    public int FailedItems { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public TimeSpan Duration { get; set; }
    public Dictionary<int, string> ItemResults { get; set; } = new(); // ExerciseId -> Result message
}

public class ExerciseValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    public List<ValidationWarning> Warnings { get; set; } = new();
    public bool HasDuplicateName { get; set; }
    public bool HasInvalidReferences { get; set; }
}

public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class ValidationWarning
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
}

public class ExerciseManagementSummary
{
    public int TotalExercises { get; set; }
    public int ActiveExercises { get; set; }
    public int InactiveExercises { get; set; }
    public Dictionary<string, int> ByMuscleGroup { get; set; } = new();
    public Dictionary<string, int> ByEquipment { get; set; } = new();
    public Dictionary<DifficultyLevel, int> ByDifficulty { get; set; } = new();
    public Dictionary<ExerciseType, int> ByType { get; set; } = new();
    public int WithImages { get; set; }
    public int WithoutImages { get; set; }
    public int WithParentExercises { get; set; }
    public int WithChildExercises { get; set; }
    public DateTime LastUpdated { get; set; }
}

public enum BulkOperationType
{
    Activate = 0,
    Deactivate = 1,
    Delete = 2,
    ChangeDifficulty = 3,
    ChangeEquipment = 4,
    AddSecondaryMuscle = 5,
    RemoveSecondaryMuscle = 6,
    UpdateMetadata = 7,
    Export = 8
}

public enum ValidationSeverity
{
    Error = 0,
    Warning = 1,
    Info = 2
}