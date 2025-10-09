namespace GymRoutineGenerator.Core.Models
{
    /// <summary>
    /// DTO for user equipment preferences (NOT an EF entity)
    /// For EF entity, see GymRoutineGenerator.Data.Entities.UserEquipmentPreference
    /// </summary>
    public class UserEquipmentPreference
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int UserProfileId { get; set; }
        public string EquipmentType { get; set; } = string.Empty;
        public int EquipmentTypeId { get; set; }
        public bool IsAvailable { get; set; }
        public int PreferenceLevel { get; set; } // 1-5
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for user muscle group preferences (NOT an EF entity)
    /// For EF entity, see GymRoutineGenerator.Data.Entities.UserMuscleGroupPreference
    /// </summary>
    public class UserMuscleGroupPreference
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string MuscleGroup { get; set; } = string.Empty;
        public int Priority { get; set; } // 1-5
        public bool WantsToFocus { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for user physical limitations (NOT an EF entity)
    /// For EF entity, see GymRoutineGenerator.Data.Entities.UserPhysicalLimitation
    /// </summary>
    public class UserPhysicalLimitation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int UserProfileId { get; set; }
        public string LimitationType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public List<string> AffectedBodyParts { get; set; } = new();
        public List<string> RestrictedMovements { get; set; } = new();
        public List<string> ExercisesToAvoid { get; set; } = new();
        public List<string> CustomRestrictions { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public DateTime DateReported { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
