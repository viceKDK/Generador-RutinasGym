namespace GymRoutineGenerator.Data.Entities;

public class WorkoutPlan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int UserAge { get; set; }
    public string Gender { get; set; } = string.Empty;
    public int UserLevel { get; set; }
    public string UserLevelName { get; set; } = string.Empty;
    public int TrainingDaysPerWeek { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string UserLimitationsJson { get; set; } = "[]";

    public virtual ICollection<WorkoutPlanRoutine> Routines { get; set; } = new List<WorkoutPlanRoutine>();
}
