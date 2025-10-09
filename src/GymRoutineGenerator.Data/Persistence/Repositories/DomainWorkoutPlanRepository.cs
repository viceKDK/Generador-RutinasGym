using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Domain.Repositories;
using GymRoutineGenerator.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

using DomainWorkoutPlan = GymRoutineGenerator.Domain.Aggregates.WorkoutPlan;
using DomainRoutine = GymRoutineGenerator.Domain.Aggregates.Routine;
using DomainExercise = GymRoutineGenerator.Domain.Aggregates.Exercise;
using DomainExerciseSet = GymRoutineGenerator.Domain.ValueObjects.ExerciseSet;
using DomainEquipmentType = GymRoutineGenerator.Domain.ValueObjects.EquipmentType;
using DataWorkoutPlan = GymRoutineGenerator.Data.Entities.WorkoutPlan;
using DataWorkoutPlanRoutine = GymRoutineGenerator.Data.Entities.WorkoutPlanRoutine;
using DataWorkoutPlanRoutineExercise = GymRoutineGenerator.Data.Entities.WorkoutPlanRoutineExercise;

namespace GymRoutineGenerator.Data.Persistence.Repositories;

public class DomainWorkoutPlanRepository : IWorkoutPlanRepository
{
    private readonly GymRoutineContext _context;
    private readonly DomainExerciseRepository _exerciseRepository;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public DomainWorkoutPlanRepository(GymRoutineContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _exerciseRepository = new DomainExerciseRepository(_context);
    }

    public async Task<DomainWorkoutPlan?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var efPlan = await LoadWorkoutPlanQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return efPlan == null
            ? null
            : await MapToDomainAsync(efPlan, cancellationToken);
    }

    public async Task<IEnumerable<DomainWorkoutPlan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var efPlans = await LoadWorkoutPlanQuery().ToListAsync(cancellationToken);
        var result = new List<DomainWorkoutPlan>(efPlans.Count);

        foreach (var efPlan in efPlans)
        {
            var domainPlan = await MapToDomainAsync(efPlan, cancellationToken);
            if (domainPlan != null)
            {
                result.Add(domainPlan);
            }
        }

        return result;
    }

    public async Task<IEnumerable<DomainWorkoutPlan>> GetByUserNameAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        var efPlans = await LoadWorkoutPlanQuery()
            .Where(p => p.UserName == userName)
            .ToListAsync(cancellationToken);

        var result = new List<DomainWorkoutPlan>(efPlans.Count);
        foreach (var efPlan in efPlans)
        {
            var domainPlan = await MapToDomainAsync(efPlan, cancellationToken);
            if (domainPlan != null)
            {
                result.Add(domainPlan);
            }
        }

        return result;
    }

    public async Task<DomainWorkoutPlan?> GetLatestByUserNameAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        var efPlan = await LoadWorkoutPlanQuery()
            .Where(p => p.UserName == userName)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return efPlan == null
            ? null
            : await MapToDomainAsync(efPlan, cancellationToken);
    }

    public async Task<DomainWorkoutPlan> AddAsync(DomainWorkoutPlan plan, CancellationToken cancellationToken = default)
    {
        if (plan == null)
            throw new ArgumentNullException(nameof(plan));

        var entity = await MapToEntityAsync(plan, cancellationToken);
        _context.WorkoutPlans.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task UpdateAsync(DomainWorkoutPlan plan, CancellationToken cancellationToken = default)
    {
        if (plan == null)
            throw new ArgumentNullException(nameof(plan));

        var existing = await _context.WorkoutPlans
            .Include(p => p.Routines)
                .ThenInclude(r => r.Exercises)
            .FirstOrDefaultAsync(p => p.Id == plan.Id, cancellationToken);

        if (existing == null)
            throw new InvalidOperationException($"WorkoutPlan with ID {plan.Id} not found");

        await UpdateEntityAsync(existing, plan, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.WorkoutPlans
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (existing != null)
        {
            _context.WorkoutPlans.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private IQueryable<DataWorkoutPlan> LoadWorkoutPlanQuery()
    {
        return _context.WorkoutPlans
            .Include(p => p.Routines)
                .ThenInclude(r => r.Exercises)
                    .ThenInclude(re => re.Exercise!)
                        .ThenInclude(e => e.PrimaryMuscleGroup)
            .Include(p => p.Routines)
                .ThenInclude(r => r.Exercises)
                    .ThenInclude(re => re.Exercise!)
                        .ThenInclude(e => e.EquipmentType)
            .Include(p => p.Routines)
                .ThenInclude(r => r.Exercises)
                    .ThenInclude(re => re.Exercise!)
                        .ThenInclude(e => e.SecondaryMuscles)
                            .ThenInclude(sm => sm.MuscleGroup)
            .AsNoTracking();
    }

    private async Task<DomainWorkoutPlan?> MapToDomainAsync(DataWorkoutPlan efPlan, CancellationToken cancellationToken)
    {
        var userLevel = MapDifficultyLevel(efPlan.UserLevel, efPlan.UserLevelName);

        var plan = DomainWorkoutPlan.Create(
            efPlan.Name,
            efPlan.UserName,
            efPlan.UserAge,
            efPlan.Gender,
            userLevel,
            efPlan.TrainingDaysPerWeek,
            efPlan.Description);

        SetProperty(plan, nameof(DomainWorkoutPlan.Id), efPlan.Id);
        SetProperty(plan, nameof(DomainWorkoutPlan.CreatedAt), efPlan.CreatedAt);
        SetProperty(plan, nameof(DomainWorkoutPlan.ModifiedAt), efPlan.ModifiedAt);

        var limitations = DeserializeLimitations(efPlan.UserLimitationsJson);
        foreach (var limitation in limitations)
        {
            // Use reflection to avoid mutating ModifiedAt per limitation
            AddLimitationWithoutTimestamp(plan, limitation);
        }

        var orderedRoutines = efPlan.Routines
            .OrderBy(r => r.DayNumber)
            .ThenBy(r => r.Id)
            .ToList();

        foreach (var efRoutine in orderedRoutines)
        {
            var routine = DomainRoutine.Create(
                efRoutine.Name,
                efRoutine.DayNumber,
                efRoutine.Description);

            SetProperty(routine, nameof(DomainRoutine.Id), efRoutine.Id);

            var orderedExercises = efRoutine.Exercises
                .OrderBy(e => e.Order)
                .ThenBy(e => e.Id)
                .ToList();

            foreach (var efRoutineExercise in orderedExercises)
            {
                var exercise = await ResolveExerciseAsync(efRoutineExercise, cancellationToken);
                var sets = DeserializeSets(efRoutineExercise.SetsJson);
                routine.AddExercise(exercise, efRoutineExercise.Order, sets, efRoutineExercise.Notes);
            }

            plan.AddRoutine(routine);
        }

        // Restore original ModifiedAt after routine/limitation adds
        SetProperty(plan, nameof(DomainWorkoutPlan.ModifiedAt), efPlan.ModifiedAt);

        return plan;
    }

    private async Task<DomainExercise> ResolveExerciseAsync(DataWorkoutPlanRoutineExercise efRoutineExercise, CancellationToken cancellationToken)
    {
        if (efRoutineExercise.ExerciseId.HasValue)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(efRoutineExercise.ExerciseId.Value, cancellationToken);
            if (exercise != null)
            {
                return exercise;
            }
        }

        // Fallback for custom or missing exercises
        var fallback = DomainExercise.Create(
            efRoutineExercise.ExerciseName,
            DomainEquipmentType.PesoCorporal,
            DifficultyLevel.Principiante,
            null);

        if (efRoutineExercise.ExerciseId.HasValue)
        {
            SetProperty(fallback, nameof(DomainExercise.Id), efRoutineExercise.ExerciseId.Value);
        }

        return fallback;
    }

    private async Task<DataWorkoutPlan> MapToEntityAsync(DomainWorkoutPlan plan, CancellationToken cancellationToken)
    {
        var entity = new DataWorkoutPlan
        {
            Name = plan.Name,
            Description = plan.Description,
            UserName = plan.UserName,
            UserAge = plan.UserAge,
            Gender = plan.Gender,
            UserLevel = plan.UserLevel.Level,
            UserLevelName = plan.UserLevel.Name,
            TrainingDaysPerWeek = plan.TrainingDaysPerWeek,
            CreatedAt = plan.CreatedAt,
            ModifiedAt = plan.ModifiedAt,
            UserLimitationsJson = SerializeLimitations(plan.UserLimitations)
        };

        foreach (var routine in plan.Routines)
        {
            var routineEntity = await MapRoutineToEntityAsync(routine, cancellationToken);
            entity.Routines.Add(routineEntity);
        }

        return entity;
    }

    private async Task UpdateEntityAsync(DataWorkoutPlan entity, DomainWorkoutPlan plan, CancellationToken cancellationToken)
    {
        entity.Name = plan.Name;
        entity.Description = plan.Description;
        entity.UserName = plan.UserName;
        entity.UserAge = plan.UserAge;
        entity.Gender = plan.Gender;
        entity.UserLevel = plan.UserLevel.Level;
        entity.UserLevelName = plan.UserLevel.Name;
        entity.TrainingDaysPerWeek = plan.TrainingDaysPerWeek;
        entity.CreatedAt = plan.CreatedAt;
        entity.ModifiedAt = plan.ModifiedAt;
        entity.UserLimitationsJson = SerializeLimitations(plan.UserLimitations);

        _context.WorkoutPlanRoutineExercises.RemoveRange(entity.Routines.SelectMany(r => r.Exercises));
        _context.WorkoutPlanRoutines.RemoveRange(entity.Routines);

        entity.Routines.Clear();

        foreach (var routine in plan.Routines)
        {
            var routineEntity = await MapRoutineToEntityAsync(routine, cancellationToken);
            routineEntity.WorkoutPlanId = entity.Id;
            entity.Routines.Add(routineEntity);
        }
    }

    private async Task<DataWorkoutPlanRoutine> MapRoutineToEntityAsync(DomainRoutine routine, CancellationToken cancellationToken)
    {
        var routineEntity = new DataWorkoutPlanRoutine
        {
            Name = routine.Name,
            Description = routine.Description,
            DayNumber = routine.DayNumber
        };

        foreach (var routineExercise in routine.Exercises)
        {
            var exerciseEntity = new DataWorkoutPlanRoutineExercise
            {
                ExerciseId = routineExercise.Exercise.Id == 0 ? null : routineExercise.Exercise.Id,
                ExerciseName = routineExercise.Exercise.Name,
                Order = routineExercise.Order,
                SetsJson = SerializeSets(routineExercise.Sets),
                Notes = routineExercise.Notes
            };

            if (routineExercise.Exercise.Id != 0)
            {
                var exists = await _context.Exercises.AnyAsync(e => e.Id == routineExercise.Exercise.Id, cancellationToken);
                if (!exists)
                {
                    exerciseEntity.ExerciseId = null;
                }
            }

            routineEntity.Exercises.Add(exerciseEntity);
        }

        return routineEntity;
    }

    private static DifficultyLevel MapDifficultyLevel(int level, string? name)
    {
        return level switch
        {
            1 => DifficultyLevel.Principiante,
            2 => DifficultyLevel.PrincipianteAvanzado,
            3 => DifficultyLevel.Intermedio,
            4 => DifficultyLevel.Avanzado,
            5 => DifficultyLevel.Experto,
            _ => DifficultyLevel.Create(name ?? $"Nivel {level}", Math.Clamp(level, 1, 5))
        };
    }

    private static List<string> DeserializeLimitations(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<string>();
        }

        return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? new List<string>();
    }

    private static string SerializeLimitations(IEnumerable<string> limitations)
    {
        return JsonSerializer.Serialize(limitations, JsonOptions);
    }

    private static List<DomainExerciseSet> DeserializeSets(string? setsJson)
    {
        if (string.IsNullOrWhiteSpace(setsJson))
        {
            return new List<DomainExerciseSet>();
        }

        var dtos = JsonSerializer.Deserialize<List<ExerciseSetDto>>(setsJson, JsonOptions) ?? new List<ExerciseSetDto>();
        return dtos.Select(dto => DomainExerciseSet.Create(dto.Repetitions, dto.Weight, dto.RestSeconds, dto.Notes)).ToList();
    }

    private static string SerializeSets(IEnumerable<DomainExerciseSet> sets)
    {
        var dtos = sets.Select(s => new ExerciseSetDto(s.Repetitions, s.Weight, s.RestSeconds, s.Notes)).ToList();
        return JsonSerializer.Serialize(dtos, JsonOptions);
    }

    private static void SetProperty<T>(object target, string propertyName, T value)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property?.SetValue(target, value);
    }

    private static void AddLimitationWithoutTimestamp(DomainWorkoutPlan plan, string limitation)
    {
        var limitationsField = typeof(DomainWorkoutPlan)
            .GetField("_userLimitations", BindingFlags.Instance | BindingFlags.NonPublic);

        if (limitationsField?.GetValue(plan) is IList<string> limitations)
        {
            if (!limitations.Contains(limitation))
            {
                limitations.Add(limitation);
            }
        }
    }

    private sealed record ExerciseSetDto(int Repetitions, int? Weight, int RestSeconds, string? Notes);
}
