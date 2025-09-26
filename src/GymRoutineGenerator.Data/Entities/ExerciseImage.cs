namespace GymRoutineGenerator.Data.Entities;

public class ExerciseImage
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string ImagePosition { get; set; } = string.Empty; // "start", "mid", "end", "demonstration"
    public bool IsPrimary { get; set; } = false;
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    public virtual Exercise Exercise { get; set; } = null!;
}