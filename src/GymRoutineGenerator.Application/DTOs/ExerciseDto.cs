namespace GymRoutineGenerator.Application.DTOs;

/// <summary>
/// DTO para transferir información de ejercicios
/// </summary>
public class ExerciseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Equipment { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int DifficultyLevel { get; set; }
    public List<string> TargetMuscles { get; set; } = new();
    public List<string> SecondaryMuscles { get; set; } = new();
    public List<string> ImagePaths { get; set; } = new();
    public bool IsActive { get; set; }
}
