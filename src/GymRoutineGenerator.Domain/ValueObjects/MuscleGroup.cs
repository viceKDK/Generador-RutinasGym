using GymRoutineGenerator.Domain.Common;

namespace GymRoutineGenerator.Domain.ValueObjects;

/// <summary>
/// Value Object que representa un grupo muscular
/// </summary>
public sealed class MuscleGroup : ValueObject
{
    public string Name { get; private set; }
    public string SpanishName { get; private set; }
    public MuscleGroupCategory Category { get; private set; }

    private MuscleGroup(string name, string spanishName, MuscleGroupCategory category)
    {
        Name = name;
        SpanishName = spanishName;
        Category = category;
    }

    public static MuscleGroup Create(string name, string spanishName, MuscleGroupCategory category)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del grupo muscular no puede estar vacío", nameof(name));

        if (string.IsNullOrWhiteSpace(spanishName))
            throw new ArgumentException("El nombre en español del grupo muscular no puede estar vacío", nameof(spanishName));

        return new MuscleGroup(name.Trim(), spanishName.Trim(), category);
    }

    // Grupos musculares predefinidos
    public static MuscleGroup Pecho => new("Chest", "Pecho", MuscleGroupCategory.Upper);
    public static MuscleGroup Espalda => new("Back", "Espalda", MuscleGroupCategory.Upper);
    public static MuscleGroup Hombros => new("Shoulders", "Hombros", MuscleGroupCategory.Upper);
    public static MuscleGroup Biceps => new("Biceps", "Bíceps", MuscleGroupCategory.Upper);
    public static MuscleGroup Triceps => new("Triceps", "Tríceps", MuscleGroupCategory.Upper);
    public static MuscleGroup Cuadriceps => new("Quadriceps", "Cuádriceps", MuscleGroupCategory.Lower);
    public static MuscleGroup Isquiotibiales => new("Hamstrings", "Isquiotibiales", MuscleGroupCategory.Lower);
    public static MuscleGroup Gluteos => new("Glutes", "Glúteos", MuscleGroupCategory.Lower);
    public static MuscleGroup Pantorrillas => new("Calves", "Pantorrillas", MuscleGroupCategory.Lower);
    public static MuscleGroup Abdominales => new("Abdominals", "Abdominales", MuscleGroupCategory.Core);
    public static MuscleGroup Lumbares => new("Lower Back", "Lumbares", MuscleGroupCategory.Core);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name.ToLowerInvariant();
        yield return SpanishName.ToLowerInvariant();
        yield return Category;
    }

    public override string ToString() => SpanishName;
}

public enum MuscleGroupCategory
{
    Upper,      // Tren superior
    Lower,      // Tren inferior
    Core        // Core/Núcleo
}
