namespace GymRoutineGenerator.Data.Entities;

public class MuscleGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SpanishName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<Exercise> PrimaryMuscleExercises { get; set; } = new List<Exercise>();
    public virtual ICollection<ExerciseSecondaryMuscle> SecondaryMuscleExercises { get; set; } = new List<ExerciseSecondaryMuscle>();
}