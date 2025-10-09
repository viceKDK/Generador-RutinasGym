using GymRoutineGenerator.Domain.Common;

namespace GymRoutineGenerator.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el tipo de equipo necesario para un ejercicio
/// </summary>
public sealed class EquipmentType : ValueObject
{
    public string Name { get; private set; }
    public string SpanishName { get; private set; }
    public EquipmentAvailability Availability { get; private set; }

    private EquipmentType(string name, string spanishName, EquipmentAvailability availability)
    {
        Name = name;
        SpanishName = spanishName;
        Availability = availability;
    }

    public static EquipmentType Create(string name, string spanishName, EquipmentAvailability availability)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del equipo no puede estar vacío", nameof(name));

        if (string.IsNullOrWhiteSpace(spanishName))
            throw new ArgumentException("El nombre en español del equipo no puede estar vacío", nameof(spanishName));

        return new EquipmentType(name.Trim(), spanishName.Trim(), availability);
    }

    // Equipos predefinidos
    public static EquipmentType PesoCorporal => new("Bodyweight", "Peso Corporal", EquipmentAvailability.Always);
    public static EquipmentType Mancuernas => new("Dumbbells", "Mancuernas", EquipmentAvailability.Common);
    public static EquipmentType Barra => new("Barbell", "Barra", EquipmentAvailability.Common);
    public static EquipmentType Maquina => new("Machine", "Máquina", EquipmentAvailability.Gym);
    public static EquipmentType Kettlebell => new("Kettlebell", "Kettlebell", EquipmentAvailability.Common);
    public static EquipmentType BandaElastica => new("Resistance Band", "Banda Elástica", EquipmentAvailability.Always);
    public static EquipmentType Polea => new("Cable Machine", "Polea", EquipmentAvailability.Gym);
    public static EquipmentType BarraZ => new("EZ Bar", "Barra Z", EquipmentAvailability.Gym);
    public static EquipmentType TRX => new("TRX", "TRX", EquipmentAvailability.Specialized);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name.ToLowerInvariant();
        yield return SpanishName.ToLowerInvariant();
        yield return Availability;
    }

    public override string ToString() => SpanishName;
}

public enum EquipmentAvailability
{
    Always,         // Siempre disponible (peso corporal, bandas)
    Common,         // Común en gimnasios y casas (mancuernas, barras)
    Gym,            // Solo en gimnasios (máquinas, poleas)
    Specialized     // Especializado (TRX, equipos específicos)
}
