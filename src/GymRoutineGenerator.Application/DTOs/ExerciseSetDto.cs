namespace GymRoutineGenerator.Application.DTOs;

/// <summary>
/// DTO para serie de ejercicio
/// </summary>
public class ExerciseSetDto
{
    public int Repetitions { get; set; }
    public int? Weight { get; set; }
    public int RestSeconds { get; set; }
    public string? Notes { get; set; }
}
