using System.ComponentModel.DataAnnotations;

namespace GymRoutineGenerator.Data.Entities;

public class UserProfile
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Gender Gender { get; set; }

    [Required]
    [Range(16, 100, ErrorMessage = "La edad debe estar entre 16 y 100 años")]
    public int Age { get; set; }

    [Required]
    [Range(1, 7, ErrorMessage = "Los días de entrenamiento deben estar entre 1 y 7")]
    public int TrainingDaysPerWeek { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public List<UserEquipmentPreference> EquipmentPreferences { get; set; } = new();
    public List<UserMuscleGroupPreference> MuscleGroupPreferences { get; set; } = new();
    public List<UserPhysicalLimitation> PhysicalLimitations { get; set; } = new();
}

public enum Gender
{
    Hombre = 1,
    Mujer = 2,
    Otro = 3
}