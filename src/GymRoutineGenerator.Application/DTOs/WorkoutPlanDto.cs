namespace GymRoutineGenerator.Application.DTOs;

/// <summary>
/// DTO para plan de entrenamiento completo
/// </summary>
public class WorkoutPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int UserAge { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string UserLevel { get; set; } = string.Empty;
    public int UserLevelNumeric { get; set; }
    public int TrainingDaysPerWeek { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public List<RoutineDto> Routines { get; set; } = new();
    public List<string> UserLimitations { get; set; } = new();
    public int TotalExercises { get; set; }
    public int TotalSets { get; set; }
    public bool IsComplete { get; set; }
}
