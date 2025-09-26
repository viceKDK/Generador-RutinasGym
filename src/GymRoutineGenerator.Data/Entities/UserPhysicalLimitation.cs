using System.ComponentModel.DataAnnotations;

namespace GymRoutineGenerator.Data.Entities;

public class UserPhysicalLimitation
{
    public int Id { get; set; }

    [Required]
    public int UserProfileId { get; set; }

    [Required]
    public LimitationType LimitationType { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(1000)]
    public string? CustomRestrictions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public UserProfile UserProfile { get; set; } = null!;
}

public enum LimitationType
{
    ProblemasEspalda = 1,
    ProblemasRodilla = 2,
    ProblemasHombro = 3,
    ProblemasCardivasculares = 4,
    LesionReciente = 5,
    Embarazo = 6,
    Artritis = 7,
    Personalizada = 99
}