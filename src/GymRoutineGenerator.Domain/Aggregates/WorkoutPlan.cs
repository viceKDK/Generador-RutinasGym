using GymRoutineGenerator.Domain.Common;
using GymRoutineGenerator.Domain.ValueObjects;

namespace GymRoutineGenerator.Domain.Aggregates;

/// <summary>
/// Agregado raíz para Plan de Entrenamiento (conjunto de rutinas)
/// </summary>
public class WorkoutPlan : Entity
{
    private readonly List<Routine> _routines = new();
    private readonly List<string> _userLimitations = new();

    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string UserName { get; private set; }
    public int UserAge { get; private set; }
    public string Gender { get; private set; }
    public DifficultyLevel UserLevel { get; private set; }
    public int TrainingDaysPerWeek { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public IReadOnlyCollection<Routine> Routines => _routines.AsReadOnly();
    public IReadOnlyCollection<string> UserLimitations => _userLimitations.AsReadOnly();

    private WorkoutPlan()
    {
        Name = string.Empty;
        UserName = string.Empty;
        Gender = string.Empty;
        UserLevel = DifficultyLevel.Principiante;
    }

    private WorkoutPlan(
        string name,
        string userName,
        int userAge,
        string gender,
        DifficultyLevel userLevel,
        int trainingDaysPerWeek,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del plan no puede estar vacío", nameof(name));

        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("El nombre del usuario no puede estar vacío", nameof(userName));

        if (userAge < 1 || userAge > 120)
            throw new ArgumentException("La edad debe estar entre 1 y 120", nameof(userAge));

        if (trainingDaysPerWeek < 1 || trainingDaysPerWeek > 7)
            throw new ArgumentException("Los días de entrenamiento deben estar entre 1 y 7", nameof(trainingDaysPerWeek));

        Name = name;
        UserName = userName;
        UserAge = userAge;
        Gender = gender;
        UserLevel = userLevel ?? throw new ArgumentNullException(nameof(userLevel));
        TrainingDaysPerWeek = trainingDaysPerWeek;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public static WorkoutPlan Create(
        string name,
        string userName,
        int userAge,
        string gender,
        DifficultyLevel userLevel,
        int trainingDaysPerWeek,
        string? description = null)
    {
        var plan = new WorkoutPlan(name, userName, userAge, gender, userLevel, trainingDaysPerWeek, description);

        // Agregar evento de dominio
        // plan.AddDomainEvent(new WorkoutPlanCreatedEvent(plan));

        return plan;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del plan no puede estar vacío", nameof(name));

        Name = name;
        ModifiedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        ModifiedAt = DateTime.UtcNow;
    }

    public void AddRoutine(Routine routine)
    {
        if (routine == null)
            throw new ArgumentNullException(nameof(routine));

        // Validar que no exceda los días de entrenamiento
        if (_routines.Count >= TrainingDaysPerWeek)
            throw new InvalidOperationException($"No se pueden agregar más de {TrainingDaysPerWeek} rutinas al plan");

        _routines.Add(routine);
        ModifiedAt = DateTime.UtcNow;
    }

    public void RemoveRoutine(int routineId)
    {
        var routine = _routines.FirstOrDefault(r => r.Id == routineId);
        if (routine != null)
        {
            _routines.Remove(routine);
            ModifiedAt = DateTime.UtcNow;
        }
    }

    public void AddUserLimitation(string limitation)
    {
        if (string.IsNullOrWhiteSpace(limitation))
            throw new ArgumentException("La limitación no puede estar vacía", nameof(limitation));

        if (!_userLimitations.Contains(limitation))
        {
            _userLimitations.Add(limitation);
            ModifiedAt = DateTime.UtcNow;
        }
    }

    public void RemoveUserLimitation(string limitation)
    {
        _userLimitations.Remove(limitation);
        ModifiedAt = DateTime.UtcNow;
    }

    public int GetTotalExercises()
    {
        return _routines.Sum(r => r.GetTotalExercises());
    }

    public int GetTotalSets()
    {
        return _routines.Sum(r => r.GetTotalSets());
    }

    public IEnumerable<MuscleGroup> GetAllTargetedMuscleGroups()
    {
        return _routines
            .SelectMany(r => r.GetTargetedMuscleGroups())
            .Distinct();
    }

    public bool IsComplete()
    {
        return _routines.Count == TrainingDaysPerWeek && _routines.All(r => r.Exercises.Count > 0);
    }

    public void UpdateUserLevel(DifficultyLevel newLevel)
    {
        UserLevel = newLevel ?? throw new ArgumentNullException(nameof(newLevel));
        ModifiedAt = DateTime.UtcNow;
    }
}
