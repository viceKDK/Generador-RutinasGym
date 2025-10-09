using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Core.Models;
using System.Text.Json;

namespace GymRoutineGenerator.Infrastructure.Mapping;

/// <summary>
/// Maps between Data.Entities and Core.Models to bridge the Data and Core layers
/// </summary>
public static class EntityMapper
{
    public static Core.Models.Exercise ToModel(this Data.Entities.Exercise entity)
    {
        if (entity == null) return null!;

        return new Core.Models.Exercise
        {
            Id = entity.Id,
            Name = entity.Name,
            SpanishName = entity.SpanishName,
            Description = entity.Description,
            Instructions = entity.Instructions,
            PrimaryMuscleGroup = entity.PrimaryMuscleGroup?.Name ?? string.Empty,
            MuscleGroups = new List<string> { entity.PrimaryMuscleGroup?.Name ?? string.Empty }
                .Concat(entity.SecondaryMuscles?.Select(sm => sm.MuscleGroup?.Name ?? string.Empty) ?? Enumerable.Empty<string>())
                .Where(m => !string.IsNullOrEmpty(m))
                .ToList(),
            Equipment = entity.EquipmentType?.Name ?? string.Empty,
            RequiredEquipment = new List<string> { entity.EquipmentType?.Name ?? string.Empty }
                .Where(e => !string.IsNullOrEmpty(e))
                .ToList(),
            DifficultyLevel = entity.DifficultyLevel.ToString(),
            ExerciseType = MapExerciseTypeFromEnum(entity.ExerciseType),
            Type = entity.ExerciseType.ToString(),
            Tags = new List<string>(),
            Metadata = new Dictionary<string, object>(),
            CreatedAt = entity.CreatedAt,
            RecommendedSets = 3,
            RecommendedReps = "8-12",
            RestPeriod = "60-90 seconds",
            Modifications = new List<string>(),
            SafetyNotes = new List<string>()
        };
    }

    public static Core.Models.UserProfile ToModel(this Data.Entities.UserProfile entity)
    {
        if (entity == null) return null!;

        return new Core.Models.UserProfile
        {
            Id = entity.Id,
            UserId = entity.Id,
            Name = entity.Name,
            Age = entity.Age,
            Gender = entity.Gender.ToString(),
            FitnessLevel = entity.FitnessLevel,
            TrainingDays = entity.TrainingDaysPerWeek,
            TrainingDaysPerWeek = entity.TrainingDaysPerWeek,
            Goals = entity.Goals ?? new List<string>(),
            CreatedDate = entity.CreatedAt,
            ModifiedDate = entity.UpdatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsActive = entity.IsActive,
            PhysicalLimitations = entity.PhysicalLimitations?.Select(pl => pl.Description).ToList() ?? new List<string>(),
            ExperienceLevel = entity.FitnessLevel,
            InjuryHistory = new List<string>()
        };
    }

    public static Core.Models.UserRoutine ToModel(this Data.Entities.UserRoutine entity)
    {
        if (entity == null) return null!;

        return new Core.Models.UserRoutine
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Name = entity.Name,
            Description = entity.Description ?? string.Empty,
            Exercises = new List<Core.Models.Exercise>(), // Will be populated from RoutineExercises if needed
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate,
            IsActive = entity.IsActive,
            Difficulty = entity.Difficulty,
            EstimatedDuration = entity.EstimatedDuration,
            RoutineType = entity.RoutineType ?? string.Empty,
            Notes = entity.Notes ?? string.Empty,
            RoutineData = entity.RoutineData ?? string.Empty,
            CreatedAt = entity.CreatedAt,
            LastModified = entity.LastModified,
            LastUpdated = entity.LastModified,
            UserName = entity.UserName ?? string.Empty,
            Goals = entity.Goals ?? new List<string>(),
            Rating = entity.Rating ?? 0,
            Status = MapRoutineStatus(entity.Status),
            IsFavorite = entity.IsFavorite,
            UserIdString = entity.UserIdString ?? entity.UserId.ToString(),
            RoutineContent = entity.RoutineContent ?? string.Empty,
            Age = entity.Age,
            Gender = entity.Gender ?? string.Empty,
            FitnessLevel = entity.FitnessLevel ?? string.Empty,
            TrainingDays = entity.TrainingDays
        };
    }

    private static Core.Models.ExerciseType MapExerciseTypeFromEnum(Core.Enums.ExerciseType type)
    {
        return type switch
        {
            Core.Enums.ExerciseType.Strength => Core.Models.ExerciseType.Compound,
            Core.Enums.ExerciseType.Cardio => Core.Models.ExerciseType.Cardio,
            Core.Enums.ExerciseType.Flexibility => Core.Models.ExerciseType.Flexibility,
            Core.Enums.ExerciseType.Balance => Core.Models.ExerciseType.Balance,
            Core.Enums.ExerciseType.Warmup => Core.Models.ExerciseType.Warmup,
            Core.Enums.ExerciseType.Cooldown => Core.Models.ExerciseType.Cooldown,
            _ => Core.Models.ExerciseType.Compound
        };
    }

    private static Core.Enums.RoutineStatus MapRoutineStatus(string status)
    {
        return status?.ToUpperInvariant() switch
        {
            "ACTIVE" => Core.Enums.RoutineStatus.Active,
            "COMPLETED" => Core.Enums.RoutineStatus.Completed,
            "ARCHIVED" => Core.Enums.RoutineStatus.Archived,
            "DRAFT" => Core.Enums.RoutineStatus.Draft,
            _ => Core.Enums.RoutineStatus.Active
        };
    }

    // Extension method for lists
    public static List<Core.Models.Exercise> ToModelList(this IEnumerable<Data.Entities.Exercise> entities)
    {
        return entities?.Select(e => e.ToModel()).Where(e => e != null).ToList() ?? new List<Core.Models.Exercise>();
    }

    public static List<Core.Models.UserProfile> ToModelList(this IEnumerable<Data.Entities.UserProfile> entities)
    {
        return entities?.Select(e => e.ToModel()).Where(e => e != null).ToList() ?? new List<Core.Models.UserProfile>();
    }

    public static List<Core.Models.UserRoutine> ToModelList(this IEnumerable<Data.Entities.UserRoutine> entities)
    {
        return entities?.Select(e => e.ToModel()).Where(e => e != null).ToList() ?? new List<Core.Models.UserRoutine>();
    }

    // Helper to convert between tuples
    public static (Core.Models.Exercise exercise, float similarity) ToModelTuple(
        this (Data.Entities.Exercise exercise, float similarity) tuple)
    {
        return (tuple.exercise.ToModel(), tuple.similarity);
    }
}