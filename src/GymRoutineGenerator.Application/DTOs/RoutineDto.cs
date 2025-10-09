namespace GymRoutineGenerator.Application.DTOs;

/// <summary>
/// DTO para rutina de un día
/// </summary>
public class RoutineDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DayNumber { get; set; }
    public List<RoutineExerciseDto> Exercises { get; set; } = new();
}

/// <summary>
/// DTO para ejercicio dentro de una rutina
/// </summary>
public class RoutineExerciseDto
{
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<ExerciseSetDto> Sets { get; set; } = new();
    public string? Notes { get; set; }
}
