using GymRoutineGenerator.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace GymRoutineGenerator.Data.Import;

public class ExerciseImportData
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string SpanishName { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Instructions { get; set; } = string.Empty;

    [Required]
    public string PrimaryMuscleGroup { get; set; } = string.Empty;

    [Required]
    public string EquipmentType { get; set; } = string.Empty;

    [Required]
    public string DifficultyLevel { get; set; } = string.Empty;

    [Required]
    public string ExerciseType { get; set; } = string.Empty;

    public int? DurationSeconds { get; set; }

    public List<string> SecondaryMuscleGroups { get; set; } = new();

    public string? ParentExerciseName { get; set; }

    public List<string> ImagePaths { get; set; } = new();

    public string? Notes { get; set; }

    public string? VideoUrl { get; set; }

    public bool IsActive { get; set; } = true;
}

public class ImportResult
{
    public bool Success { get; set; }
    public int TotalRecords { get; set; }
    public int SuccessfulImports { get; set; }
    public int FailedImports { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public TimeSpan Duration { get; set; }
}

public class ImportValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}