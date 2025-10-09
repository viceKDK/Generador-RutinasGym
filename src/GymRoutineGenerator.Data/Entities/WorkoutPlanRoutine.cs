namespace GymRoutineGenerator.Data.Entities;

public class WorkoutPlanRoutine
{
    public int Id { get; set; }
    public int WorkoutPlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DayNumber { get; set; }

    public virtual WorkoutPlan WorkoutPlan { get; set; } = null!;
    public virtual ICollection<WorkoutPlanRoutineExercise> Exercises { get; set; } = new List<WorkoutPlanRoutineExercise>();
}
