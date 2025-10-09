namespace GymRoutineGenerator.Data.Entities;

public class WorkoutPlanRoutineExercise
{
    public int Id { get; set; }
    public int WorkoutPlanRoutineId { get; set; }
    public int? ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int Order { get; set; }
    public string SetsJson { get; set; } = "[]";
    public string? Notes { get; set; }

    public virtual WorkoutPlanRoutine Routine { get; set; } = null!;
    public virtual Exercise? Exercise { get; set; }
}
