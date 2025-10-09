using System;
using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.Repositories;
using GymRoutineGenerator.Domain.ValueObjects;
using GymRoutineGenerator.Data.Context;
using Microsoft.EntityFrameworkCore;
using DataExercise = GymRoutineGenerator.Data.Entities.Exercise;

namespace GymRoutineGenerator.Infrastructure.Persistence;

/// <summary>
/// Implementación del repositorio de ejercicios usando Domain aggregates
/// Hace bridge entre Data entities y Domain aggregates
/// </summary>
public class DomainExerciseRepository : IExerciseRepository
{
    private readonly GymRoutineContext _context;

    public DomainExerciseRepository(GymRoutineContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Exercise?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var dataExercise = await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.EquipmentType)
            .Include(e => e.SecondaryMuscles)
            .ThenInclude(sm => sm.MuscleGroup)
            .Include(e => e.Images)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        return dataExercise == null ? null : MapToDomain(dataExercise);
    }

    public async Task<IEnumerable<Exercise>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var dataExercises = await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.EquipmentType)
            .Include(e => e.SecondaryMuscles)
            .ThenInclude(sm => sm.MuscleGroup)
            .Include(e => e.Images)
            .ToListAsync(cancellationToken);

        return dataExercises.Select(MapToDomain);
    }

    public async Task<IEnumerable<Exercise>> GetActiveExercisesAsync(CancellationToken cancellationToken = default)
    {
        var dataExercises = await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.EquipmentType)
            .Include(e => e.SecondaryMuscles)
            .ThenInclude(sm => sm.MuscleGroup)
            .Include(e => e.Images)
            .Where(e => e.IsActive)
            .ToListAsync(cancellationToken);

        return dataExercises.Select(MapToDomain);
    }

    public async Task<IEnumerable<Exercise>> GetByMuscleGroupAsync(MuscleGroup muscleGroup, CancellationToken cancellationToken = default)
    {
        var dataExercises = await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.EquipmentType)
            .Include(e => e.SecondaryMuscles)
            .ThenInclude(sm => sm.MuscleGroup)
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

        return dataExercises.Select(MapToDomain);
    }

    public async Task<IEnumerable<Exercise>> GetByEquipmentAsync(EquipmentType equipment, CancellationToken cancellationToken = default)
    {
        var dataExercises = await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.EquipmentType)
            .Include(e => e.SecondaryMuscles)
            .ThenInclude(sm => sm.MuscleGroup)
            .Include(e => e.Images)
            .Where(e => e.EquipmentType != null)
            .Where(e =>
                (e.EquipmentType!.Name != null &&
                 e.EquipmentType.Name.Equals(equipment.Name, StringComparison.OrdinalIgnoreCase)) ||
                (e.EquipmentType!.SpanishName != null &&
                 e.EquipmentType.SpanishName.Equals(equipment.SpanishName, StringComparison.OrdinalIgnoreCase)))
            .ToListAsync(cancellationToken);

        return dataExercises.Select(MapToDomain);
    }

    public async Task<IEnumerable<Exercise>> GetByDifficultyAsync(DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        var dataExercises = await _context.Exercises
            .Include(e => e.PrimaryMuscleGroup)
            .Include(e => e.EquipmentType)
            .Include(e => e.SecondaryMuscles)
            .ThenInclude(sm => sm.MuscleGroup)
            .Include(e => e.Images)
            .Where(e => (int)e.DifficultyLevel == difficulty.Level)
            .ToListAsync(cancellationToken);

        return dataExercises.Select(MapToDomain);
    }

    public async Task<Exercise> AddAsync(Exercise exercise, CancellationToken cancellationToken = default)
    {
        var dataExercise = MapToData(exercise);
        _context.Exercises.Add(dataExercise);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload para obtener el ID y todas las relaciones
        await _context.Entry(dataExercise).ReloadAsync(cancellationToken);
        await _context.Entry(dataExercise).Reference(e => e.PrimaryMuscleGroup).LoadAsync(cancellationToken);
        await _context.Entry(dataExercise).Reference(e => e.EquipmentType).LoadAsync(cancellationToken);
        await _context.Entry(dataExercise).Collection(e => e.SecondaryMuscles).LoadAsync(cancellationToken);
        await _context.Entry(dataExercise).Collection(e => e.Images).LoadAsync(cancellationToken);

        return MapToDomain(dataExercise);
    }

    public async Task UpdateAsync(Exercise exercise, CancellationToken cancellationToken = default)
    {
        var existingDataExercise = await _context.Exercises
            .Include(e => e.SecondaryMuscles)
            .Include(e => e.Images)
            .FirstOrDefaultAsync(e => e.Id == exercise.Id, cancellationToken);

        if (existingDataExercise == null)
        {
            throw new InvalidOperationException($"Exercise with ID {exercise.Id} not found");
        }

        // Actualizar propiedades básicas
        existingDataExercise.Name = exercise.Name;
        existingDataExercise.SpanishName = exercise.Name; // Por ahora usamos el mismo
        existingDataExercise.Description = exercise.Description;
        existingDataExercise.IsActive = exercise.IsActive;

        // Actualizar Equipment y Difficulty requiere buscar los IDs correspondientes
        var equipmentType = await _context.EquipmentTypes
            .FirstOrDefaultAsync(et => et.Name == exercise.Equipment.Name, cancellationToken);
        if (equipmentType != null)
        {
            existingDataExercise.EquipmentTypeId = equipmentType.Id;
        }

        existingDataExercise.DifficultyLevel = (GymRoutineGenerator.Core.Enums.DifficultyLevel)exercise.Difficulty.Level;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var dataExercise = await _context.Exercises.FindAsync(new object[] { id }, cancellationToken);
        if (dataExercise != null)
        {
            // Soft delete
            dataExercise.IsActive = false;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    #region Private Mapping Methods

    private Exercise MapToDomain(DataExercise dataExercise)
    {
        // Crear Equipment ValueObject
        var equipmentName = dataExercise.EquipmentType?.Name ?? "Bodyweight";
        var equipmentSpanishName = dataExercise.EquipmentType?.SpanishName ?? dataExercise.EquipmentType?.Name ?? "Peso Corporal";
        var equipment = EquipmentType.Create(
            equipmentName,
            equipmentSpanishName,
            EquipmentAvailability.Common
        );

        // Crear Difficulty ValueObject - mapear desde el enum al nivel
        var difficulty = (int)dataExercise.DifficultyLevel switch
        {
            1 => DifficultyLevel.Principiante,
            2 => DifficultyLevel.PrincipianteAvanzado,
            3 => DifficultyLevel.Intermedio,
            4 => DifficultyLevel.Avanzado,
            5 => DifficultyLevel.Experto,
            _ => DifficultyLevel.Principiante
        };

        // Crear Exercise con Factory Method
        var exercise = Exercise.Create(
            dataExercise.Name,
            equipment,
            difficulty,
            dataExercise.Description
        );

        // Usar reflexión para establecer el ID (ya que Exercise.Id es internal set)
        var idProperty = typeof(Exercise).GetProperty("Id");
        idProperty?.SetValue(exercise, dataExercise.Id);

        // Agregar músculos target
        if (dataExercise.PrimaryMuscleGroup != null)
        {
            var spanishName = dataExercise.PrimaryMuscleGroup.SpanishName ?? dataExercise.PrimaryMuscleGroup.Name;
            var primaryMuscle = MuscleGroup.Create(
                dataExercise.PrimaryMuscleGroup.Name,
                spanishName,
                MuscleGroupCategory.Upper // Default, podría mejorarse con lógica de mapeo
            );
            exercise.AddTargetMuscle(primaryMuscle);
        }

        // Agregar músculos secundarios
        foreach (var secondaryMuscle in dataExercise.SecondaryMuscles ?? Enumerable.Empty<Data.Entities.ExerciseSecondaryMuscle>())
        {
            if (secondaryMuscle.MuscleGroup != null)
            {
                var spanishName = secondaryMuscle.MuscleGroup.SpanishName ?? secondaryMuscle.MuscleGroup.Name;
                var muscle = MuscleGroup.Create(
                    secondaryMuscle.MuscleGroup.Name,
                    spanishName,
                    MuscleGroupCategory.Upper // Default, podría mejorarse con lógica de mapeo
                );
                exercise.AddSecondaryMuscle(muscle);
            }
        }

        // Agregar image paths
        foreach (var image in dataExercise.Images ?? Enumerable.Empty<Data.Entities.ExerciseImage>())
        {
            if (!string.IsNullOrEmpty(image.ImagePath))
            {
                exercise.AddImagePath(image.ImagePath);
            }
        }

        // Establecer IsActive
        if (!dataExercise.IsActive)
        {
            exercise.Deactivate();
        }

        return exercise;
    }

    private DataExercise MapToData(Exercise exercise)
    {
        // Este mapeo es más complejo y requiere buscar/crear entidades relacionadas
        // Por ahora, una implementación simplificada
        var dataExercise = new DataExercise
        {
            Name = exercise.Name,
            SpanishName = exercise.Name,
            Description = exercise.Description,
            IsActive = exercise.IsActive,
            DifficultyLevel = (GymRoutineGenerator.Core.Enums.DifficultyLevel)exercise.Difficulty.Level
        };

        // Buscar EquipmentType existente (esto debería hacerse de forma asíncrona idealmente)
        var equipmentType = _context.EquipmentTypes
            .FirstOrDefault(et =>
                string.Equals(et.Name, exercise.Equipment.Name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(et.SpanishName, exercise.Equipment.SpanishName, StringComparison.OrdinalIgnoreCase));
        if (equipmentType != null)
        {
            dataExercise.EquipmentTypeId = equipmentType.Id;
        }

        // Buscar PrimaryMuscleGroup
        var primaryMuscle = exercise.TargetMuscles.FirstOrDefault();
        if (primaryMuscle != null)
        {
            var muscleGroup = _context.MuscleGroups
                .FirstOrDefault(mg =>
                    string.Equals(mg.Name, primaryMuscle.Name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(mg.SpanishName, primaryMuscle.SpanishName, StringComparison.OrdinalIgnoreCase));
            if (muscleGroup != null)
            {
                dataExercise.PrimaryMuscleGroupId = muscleGroup.Id;
            }
        }

        return dataExercise;
    }

    #endregion
}
