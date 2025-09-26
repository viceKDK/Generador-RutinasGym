using GymRoutineGenerator.Core.Enums;

namespace GymRoutineGenerator.Data.Entities;

public class Exercise
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SpanishName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;

    // Foreign keys
    public int PrimaryMuscleGroupId { get; set; }
    public int EquipmentTypeId { get; set; }
    public int? ParentExerciseId { get; set; }

    // Enums
    public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Beginner;
    public ExerciseType ExerciseType { get; set; } = ExerciseType.Strength;

    // Metadata
    public int? DurationSeconds { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual MuscleGroup PrimaryMuscleGroup { get; set; } = null!;
    public virtual EquipmentType EquipmentType { get; set; } = null!;
    public virtual Exercise? ParentExercise { get; set; }
    public virtual ICollection<Exercise> ChildExercises { get; set; } = new List<Exercise>();
    public virtual ICollection<ExerciseImage> Images { get; set; } = new List<ExerciseImage>();
    public virtual ICollection<ExerciseSecondaryMuscle> SecondaryMuscles { get; set; } = new List<ExerciseSecondaryMuscle>();
}