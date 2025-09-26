namespace GymRoutineGenerator.Data.Entities;

public class EquipmentType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SpanishName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}