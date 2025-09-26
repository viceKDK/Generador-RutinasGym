using System.ComponentModel.DataAnnotations;

namespace GymRoutineGenerator.Data.Entities;

public class UserEquipmentPreference
{
    public int Id { get; set; }

    [Required]
    public int UserProfileId { get; set; }

    [Required]
    public int EquipmentTypeId { get; set; }

    public bool IsAvailable { get; set; } = true;

    // Navigation properties
    public UserProfile UserProfile { get; set; } = null!;
    public EquipmentType EquipmentType { get; set; } = null!;
}