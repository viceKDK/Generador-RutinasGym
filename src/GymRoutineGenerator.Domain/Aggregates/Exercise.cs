using GymRoutineGenerator.Domain.Common;
using GymRoutineGenerator.Domain.ValueObjects;

namespace GymRoutineGenerator.Domain.Aggregates;

/// <summary>
/// Agregado raíz para Ejercicio
/// </summary>
public class Exercise : Entity
{
    private readonly List<MuscleGroup> _targetMuscles = new();
    private readonly List<MuscleGroup> _secondaryMuscles = new();
    private readonly List<string> _imagePaths = new();

    public string Name { get; private set; }
    public string? Description { get; private set; }
    public EquipmentType Equipment { get; private set; }
    public DifficultyLevel Difficulty { get; private set; }
    public IReadOnlyCollection<MuscleGroup> TargetMuscles => _targetMuscles.AsReadOnly();
    public IReadOnlyCollection<MuscleGroup> SecondaryMuscles => _secondaryMuscles.AsReadOnly();
    public IReadOnlyCollection<string> ImagePaths => _imagePaths.AsReadOnly();
    public bool IsActive { get; private set; }

    private Exercise()
    {
        Name = string.Empty;
        Equipment = EquipmentType.PesoCorporal;
        Difficulty = DifficultyLevel.Principiante;
    }

    private Exercise(
        string name,
        EquipmentType equipment,
        DifficultyLevel difficulty,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del ejercicio no puede estar vacío", nameof(name));

        Name = name;
        Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
        Difficulty = difficulty ?? throw new ArgumentNullException(nameof(difficulty));
        Description = description;
        IsActive = true;
    }

    public static Exercise Create(
        string name,
        EquipmentType equipment,
        DifficultyLevel difficulty,
        string? description = null)
    {
        var exercise = new Exercise(name, equipment, difficulty, description);

        // Aquí se puede agregar un DomainEvent si es necesario
        // exercise.AddDomainEvent(new ExerciseCreatedEvent(exercise));

        return exercise;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del ejercicio no puede estar vacío", nameof(name));

        Name = name;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }

    public void UpdateEquipment(EquipmentType equipment)
    {
        Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
    }

    public void UpdateDifficulty(DifficultyLevel difficulty)
    {
        Difficulty = difficulty ?? throw new ArgumentNullException(nameof(difficulty));
    }

    public void AddTargetMuscle(MuscleGroup muscle)
    {
        if (muscle == null)
            throw new ArgumentNullException(nameof(muscle));

        if (!_targetMuscles.Contains(muscle))
        {
            _targetMuscles.Add(muscle);
        }
    }

    public void AddSecondaryMuscle(MuscleGroup muscle)
    {
        if (muscle == null)
            throw new ArgumentNullException(nameof(muscle));

        if (!_secondaryMuscles.Contains(muscle))
        {
            _secondaryMuscles.Add(muscle);
        }
    }

    public void AddImagePath(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            throw new ArgumentException("La ruta de la imagen no puede estar vacía", nameof(imagePath));

        if (!_imagePaths.Contains(imagePath))
        {
            _imagePaths.Add(imagePath);
        }
    }

    public void RemoveImagePath(string imagePath)
    {
        _imagePaths.Remove(imagePath);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public bool WorksMuscleGroup(MuscleGroup muscleGroup)
    {
        return _targetMuscles.Contains(muscleGroup) || _secondaryMuscles.Contains(muscleGroup);
    }

    public bool IsAppropriateForLevel(DifficultyLevel userLevel)
    {
        return Difficulty.IsAppropriateFor(userLevel);
    }
}
