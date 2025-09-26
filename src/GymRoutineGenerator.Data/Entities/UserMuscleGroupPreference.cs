using System.ComponentModel.DataAnnotations;

namespace GymRoutineGenerator.Data.Entities;

public class UserMuscleGroupPreference
{
    public int Id { get; set; }

    [Required]
    public int UserProfileId { get; set; }

    [Required]
    public int MuscleGroupId { get; set; }

    [Required]
    [Range(1, 3, ErrorMessage = "El nivel de énfasis debe estar entre 1 (Bajo) y 3 (Alto)")]
    public EmphasisLevel EmphasisLevel { get; set; } = EmphasisLevel.Medio;

    // Navigation properties
    public UserProfile UserProfile { get; set; } = null!;
    public MuscleGroup MuscleGroup { get; set; } = null!;
}

public enum EmphasisLevel
{
    Bajo = 1,
    Medio = 2,
    Alto = 3
}