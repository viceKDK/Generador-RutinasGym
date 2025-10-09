using System;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.Repositories;
using GymRoutineGenerator.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using EfExercise = GymRoutineGenerator.Data.Entities.Exercise;

namespace GymRoutineGenerator.Data.Persistence.Repositories;

/// <summary>
/// Repositorio que mapea entre Domain.Exercise y Data.Entities.Exercise
/// </summary>
public class DomainExerciseRepository : IExerciseRepository
{
    private readonly GymRoutineContext _context;

    public DomainExerciseRepository(GymRoutineContext context)
    {
        _context = context;
    }

    public async Task<Domain.Aggregates.Exercise?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var efExercise = await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.SecondaryMuscles)
                .ThenInclude(sm => sm.MuscleGroup)
            .Include(e => e.EquipmentType)
            .Include(e => e.Images)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        return efExercise == null ? null : MapToDomain(efExercise);
    }

    public async Task<IEnumerable<Domain.Aggregates.Exercise>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var efExercises = await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.SecondaryMuscles)
                .ThenInclude(sm => sm.MuscleGroup)
            .Include(e => e.EquipmentType)
            .Include(e => e.Images)
            .ToListAsync(cancellationToken);

        return efExercises.Select(MapToDomain);
    }

    public async Task<IEnumerable<Domain.Aggregates.Exercise>> GetActiveExercisesAsync(CancellationToken cancellationToken = default)
    {
        var efExercises = await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.SecondaryMuscles)
                .ThenInclude(sm => sm.MuscleGroup)
            .Include(e => e.EquipmentType)
            .Include(e => e.Images)
            .Where(e => e.IsActive)
            .ToListAsync(cancellationToken);

        return efExercises.Select(MapToDomain);
    }

    public async Task<IEnumerable<Domain.Aggregates.Exercise>> GetByMuscleGroupAsync(
        MuscleGroup muscleGroup,
        CancellationToken cancellationToken = default)
    {
        var efExercises = await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.SecondaryMuscles)
                .ThenInclude(sm => sm.MuscleGroup)
            .Include(e => e.EquipmentType)
            .Include(e => e.Images)
            .Where(e =>
                (e.PrimaryMuscleGroup.Name != null &&
                 e.PrimaryMuscleGroup.Name.Equals(muscleGroup.Name, StringComparison.OrdinalIgnoreCase)) ||
                (e.PrimaryMuscleGroup.SpanishName != null &&
                 e.PrimaryMuscleGroup.SpanishName.Equals(muscleGroup.SpanishName, StringComparison.OrdinalIgnoreCase)) ||
                e.SecondaryMuscles.Any(sm =>
                    (sm.MuscleGroup.Name != null &&
                     sm.MuscleGroup.Name.Equals(muscleGroup.Name, StringComparison.OrdinalIgnoreCase)) ||
                    (sm.MuscleGroup.SpanishName != null &&
                     sm.MuscleGroup.SpanishName.Equals(muscleGroup.SpanishName, StringComparison.OrdinalIgnoreCase))))
            .ToListAsync(cancellationToken);

        return efExercises.Select(MapToDomain);
    }

    public async Task<IEnumerable<Domain.Aggregates.Exercise>> GetByEquipmentAsync(
        EquipmentType equipment,
        CancellationToken cancellationToken = default)
    {
        var efExercises = await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.SecondaryMuscles)
                .ThenInclude(sm => sm.MuscleGroup)
            .Include(e => e.EquipmentType)
            .Include(e => e.Images)
            .Where(e => e.EquipmentType != null)
            .Where(e =>
                (e.EquipmentType!.Name != null &&
                 e.EquipmentType.Name.Equals(equipment.Name, StringComparison.OrdinalIgnoreCase)) ||
                (e.EquipmentType!.SpanishName != null &&
                 e.EquipmentType.SpanishName.Equals(equipment.SpanishName, StringComparison.OrdinalIgnoreCase)))
            .ToListAsync(cancellationToken);

        return efExercises.Select(MapToDomain);
    }

    public async Task<IEnumerable<Domain.Aggregates.Exercise>> GetByDifficultyAsync(
        DifficultyLevel difficulty,
        CancellationToken cancellationToken = default)
    {
        var efExercises = await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.SecondaryMuscles)
                .ThenInclude(sm => sm.MuscleGroup)
            .Include(e => e.EquipmentType)
            .Include(e => e.Images)
            .Where(e => e.DifficultyLevel == MapToDifficultyEnum(difficulty))
            .ToListAsync(cancellationToken);

        return efExercises.Select(MapToDomain);
    }

    public async Task<Domain.Aggregates.Exercise> AddAsync(Domain.Aggregates.Exercise exercise, CancellationToken cancellationToken = default)
    {
        var efExercise = await MapToEfAsync(exercise, cancellationToken);
        _context.Exercises.Add(efExercise);
        await _context.SaveChangesAsync(cancellationToken);

        // Recargar para obtener el ID y navegaciones
        await _context.Entry(efExercise)
            .Reference(e => e.PrimaryMuscleGroup)
            .LoadAsync(cancellationToken);
        await _context.Entry(efExercise)
            .Reference(e => e.EquipmentType)
            .LoadAsync(cancellationToken);
        await _context.Entry(efExercise)
            .Collection(e => e.Images)
            .LoadAsync(cancellationToken);

        return MapToDomain(efExercise);
    }

    public async Task UpdateAsync(Domain.Aggregates.Exercise exercise, CancellationToken cancellationToken = default)
    {
        var efExercise = await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.SecondaryMuscles)
            .Include(e => e.EquipmentType)
            .Include(e => e.Images)
            .FirstOrDefaultAsync(e => e.Id == exercise.Id, cancellationToken);

        if (efExercise == null)
            throw new InvalidOperationException($"Exercise with ID {exercise.Id} not found");

        await UpdateEfExerciseAsync(efExercise, exercise, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var efExercise = await _context.Exercises.FindAsync(new object[] { id }, cancellationToken);
        if (efExercise != null)
        {
            _context.Exercises.Remove(efExercise);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    // Mapeo Domain → EF
    private async Task<EfExercise> MapToEfAsync(Domain.Aggregates.Exercise domain, CancellationToken cancellationToken)
    {
        // Buscar el equipmentType en la BD
        var equipmentType = await _context.EquipmentTypes
            .FirstOrDefaultAsync(
                et => et.Name.Equals(domain.Equipment.Name, StringComparison.OrdinalIgnoreCase) ||
                      et.SpanishName.Equals(domain.Equipment.SpanishName, StringComparison.OrdinalIgnoreCase),
                cancellationToken)
            ?? throw new InvalidOperationException($"EquipmentType '{domain.Equipment.Name}' not found");

        // Buscar el primary muscle group
        var primaryMuscle = domain.TargetMuscles.FirstOrDefault();
        if (primaryMuscle == null)
            throw new InvalidOperationException("Exercise must have at least one target muscle");

        var primaryMuscleGroup = await _context.MuscleGroups
            .FirstOrDefaultAsync(
                mg => mg.Name.Equals(primaryMuscle.Name, StringComparison.OrdinalIgnoreCase) ||
                      mg.SpanishName.Equals(primaryMuscle.SpanishName, StringComparison.OrdinalIgnoreCase),
                cancellationToken)
            ?? throw new InvalidOperationException($"MuscleGroup '{primaryMuscle.Name}' not found");

        return new EfExercise
        {
            Name = domain.Name,
            SpanishName = domain.Name,
            Description = domain.Description,
            EquipmentTypeId = equipmentType.Id,
            PrimaryMuscleGroupId = primaryMuscleGroup.Id,
            DifficultyLevel = MapToDifficultyEnum(domain.Difficulty),
            IsActive = domain.IsActive
        };
    }

    // Mapeo EF → Domain
    private Domain.Aggregates.Exercise MapToDomain(EfExercise ef)
    {
        var difficulty = MapFromDifficultyEnum(ef.DifficultyLevel);
        var equipment = MapEquipmentType(ef.EquipmentType);

        var exercise = Domain.Aggregates.Exercise.Create(
            ef.Name,
            equipment,
            difficulty,
            ef.Description);

        // Usar reflexión para establecer el ID (ya que es protected set)
        typeof(Domain.Aggregates.Exercise).GetProperty("Id")!.SetValue(exercise, ef.Id);

        // Agregar músculo principal
        if (ef.PrimaryMuscleGroup != null)
        {
            var primaryMuscle = CreateMuscleGroupFromEntity(ef.PrimaryMuscleGroup);
            exercise.AddTargetMuscle(primaryMuscle);
        }

        // Agregar músculos secundarios
        if (ef.SecondaryMuscles != null)
        {
            foreach (var sm in ef.SecondaryMuscles)
            {
                if (sm.MuscleGroup != null)
                {
                    var muscleGroup = CreateMuscleGroupFromEntity(sm.MuscleGroup);
                    exercise.AddSecondaryMuscle(muscleGroup);
                }
            }
        }

        // Agregar imágenes
        if (ef.Images != null)
        {
            foreach (var img in ef.Images)
            {
                exercise.AddImagePath(img.ImagePath);
            }
        }

        if (!ef.IsActive)
        {
            exercise.Deactivate();
        }

        return exercise;
    }

    private async Task UpdateEfExerciseAsync(EfExercise ef, Domain.Aggregates.Exercise domain, CancellationToken cancellationToken)
    {
        ef.Name = domain.Name;
        ef.SpanishName = domain.Name;
        ef.Description = domain.Description;
        ef.DifficultyLevel = MapToDifficultyEnum(domain.Difficulty);
        ef.IsActive = domain.IsActive;

        // Actualizar equipment si cambió
        if (ef.EquipmentType?.Name != domain.Equipment.Name)
        {
            var equipmentType = await _context.EquipmentTypes
                .FirstOrDefaultAsync(et => et.Name == domain.Equipment.Name, cancellationToken);
            if (equipmentType != null)
            {
                ef.EquipmentTypeId = equipmentType.Id;
            }
        }
    }

    private DifficultyLevel MapFromDifficultyEnum(Core.Enums.DifficultyLevel efDifficulty)
    {
        return efDifficulty switch
        {
            Core.Enums.DifficultyLevel.Beginner => DifficultyLevel.Principiante,
            Core.Enums.DifficultyLevel.Intermediate => DifficultyLevel.Intermedio,
            Core.Enums.DifficultyLevel.Advanced => DifficultyLevel.Avanzado,
            _ => DifficultyLevel.Principiante
        };
    }

    private Core.Enums.DifficultyLevel MapToDifficultyEnum(DifficultyLevel domainDifficulty)
    {
        if (domainDifficulty == DifficultyLevel.Principiante ||
            domainDifficulty == DifficultyLevel.PrincipianteAvanzado)
            return Core.Enums.DifficultyLevel.Beginner;

        if (domainDifficulty == DifficultyLevel.Intermedio)
            return Core.Enums.DifficultyLevel.Intermediate;

        if (domainDifficulty == DifficultyLevel.Avanzado ||
            domainDifficulty == DifficultyLevel.Experto)
            return Core.Enums.DifficultyLevel.Advanced;

        return Core.Enums.DifficultyLevel.Beginner;
    }

    private EquipmentType MapEquipmentType(Data.Entities.EquipmentType? efEquipment)
    {
        if (efEquipment == null)
        {
            return EquipmentType.PesoCorporal;
        }

        var normalizedNames = GetNormalizedNames(efEquipment.Name, efEquipment.SpanishName).ToList();

        foreach (var name in normalizedNames)
        {
            switch (name)
            {
                case "bodyweight":
                case "peso corporal":
                    return EquipmentType.PesoCorporal;
                case "dumbbells":
                case "mancuernas":
                    return EquipmentType.Mancuernas;
                case "barbell":
                case "barra":
                    return EquipmentType.Barra;
                case "machine":
                case "máquina":
                case "maquina":
                    return EquipmentType.Maquina;
                case "kettlebell":
                    return EquipmentType.Kettlebell;
                case "resistance band":
                case "band":
                case "banda elástica":
                case "banda elastica":
                    return EquipmentType.BandaElastica;
                case "cable machine":
                case "polea":
                case "cables":
                    return EquipmentType.Polea;
                case "ez bar":
                case "barra z":
                    return EquipmentType.BarraZ;
                case "trx":
                    return EquipmentType.TRX;
            }
        }

        var spanishName = string.IsNullOrWhiteSpace(efEquipment.SpanishName)
            ? efEquipment.Name
            : efEquipment.SpanishName;

        var availability = DetermineEquipmentAvailability(normalizedNames);

        return EquipmentType.Create(efEquipment.Name, spanishName, availability);
    }

    private MuscleGroup CreateMuscleGroupFromEntity(Data.Entities.MuscleGroup entity)
    {
        var spanishName = string.IsNullOrWhiteSpace(entity.SpanishName)
            ? entity.Name
            : entity.SpanishName;

        var category = DetermineMuscleCategory(entity.Name, spanishName);
        return MuscleGroup.Create(entity.Name, spanishName, category);
    }

    private MuscleGroupCategory DetermineMuscleCategory(string? englishName, string? spanishName)
    {
        var names = GetNormalizedNames(englishName, spanishName).ToList();

        if (names.Any(n => UpperBodyTerms.Any(term => n.Contains(term))))
        {
            return MuscleGroupCategory.Upper;
        }

        if (names.Any(n => LowerBodyTerms.Any(term => n.Contains(term))))
        {
            return MuscleGroupCategory.Lower;
        }

        if (names.Any(n => CoreTerms.Any(term => n.Contains(term))))
        {
            return MuscleGroupCategory.Core;
        }

        return MuscleGroupCategory.Upper;
    }

    private EquipmentAvailability DetermineEquipmentAvailability(IReadOnlyCollection<string> normalizedNames)
    {
        if (normalizedNames.Count == 0)
        {
            return EquipmentAvailability.Common;
        }

        if (normalizedNames.Any(n => n.Contains("bodyweight") || n.Contains("peso corporal") || n.Contains("band")))
        {
            return EquipmentAvailability.Always;
        }

        if (normalizedNames.Any(n => n.Contains("trx") || n.Contains("suspension")))
        {
            return EquipmentAvailability.Specialized;
        }

        if (normalizedNames.Any(n => n.Contains("machine") || n.Contains("máquina") || n.Contains("maquina") || n.Contains("cable") || n.Contains("polea")))
        {
            return EquipmentAvailability.Gym;
        }

        return EquipmentAvailability.Common;
    }

    private IEnumerable<string> GetNormalizedNames(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                yield return value.Trim().ToLowerInvariant();
            }
        }
    }

    private static readonly string[] UpperBodyTerms =
    {
        "pecho", "chest", "espalda", "back", "hombro", "hombros", "shoulder", "shoulders",
        "bíceps", "biceps", "tríceps", "triceps", "deltoid", "deltoides", "trapecio", "trapezius"
    };

    private static readonly string[] LowerBodyTerms =
    {
        "pierna", "piernas", "leg", "legs", "cuádriceps", "cuadriceps", "quadricep", "quadriceps",
        "isquiotibiales", "hamstring", "hamstrings", "glúteo", "glúteos", "glute", "glutes",
        "pantorrilla", "pantorrillas", "calf", "calves"
    };

    private static readonly string[] CoreTerms =
    {
        "core", "abdomen", "abdominal", "abdominales", "abs", "lumbar", "lumbares", "oblique", "oblicuos"
    };
}
