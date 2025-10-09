using GymRoutineGenerator.Domain.Common;

namespace GymRoutineGenerator.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el nivel de dificultad de un ejercicio
/// </summary>
public sealed class DifficultyLevel : ValueObject
{
    public string Name { get; private set; }
    public int Level { get; private set; }  // 1-5

    private DifficultyLevel(string name, int level)
    {
        Name = name;
        Level = level;
    }

    public static DifficultyLevel Create(string name, int level)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del nivel no puede estar vacío", nameof(name));

        if (level < 1 || level > 5)
            throw new ArgumentException("El nivel debe estar entre 1 y 5", nameof(level));

        return new DifficultyLevel(name, level);
    }

    // Niveles predefinidos
    public static DifficultyLevel Principiante => new("Principiante", 1);
    public static DifficultyLevel PrincipianteAvanzado => new("Principiante Avanzado", 2);
    public static DifficultyLevel Intermedio => new("Intermedio", 3);
    public static DifficultyLevel Avanzado => new("Avanzado", 4);
    public static DifficultyLevel Experto => new("Experto", 5);

    public bool IsAppropriateFor(DifficultyLevel userLevel)
    {
        // Un ejercicio es apropiado si su nivel es menor o igual al del usuario
        return Level <= userLevel.Level;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Level;
    }

    public override string ToString() => $"{Name} (Nivel {Level})";
}
