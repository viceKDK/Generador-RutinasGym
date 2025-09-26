namespace GymRoutineGenerator.Data.Entities;

public class ExerciseSecondaryMuscle
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public int MuscleGroupId { get; set; }

    // Navigation properties
    public virtual Exercise Exercise { get; set; } = null!;
    public virtual MuscleGroup MuscleGroup { get; set; } = null!;
}