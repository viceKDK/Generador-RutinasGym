using System;
using System.Collections.Generic;
using System.Linq;
using GymRoutineGenerator.Domain.Aggregates;
using GymRoutineGenerator.Domain.Services;
using GymRoutineGenerator.Infrastructure.Services;
using Entities = GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Infrastructure.Services.Adapters;

/// <summary>
/// Adapter that implements Domain service interface using Infrastructure service
/// </summary>
public class WorkoutPlanGenerationServiceAdapter : IWorkoutPlanGenerationService
{
    private readonly IRoutineGenerationService _routineGenerationService;

    public WorkoutPlanGenerationServiceAdapter(IRoutineGenerationService routineGenerationService)
    {
        _routineGenerationService = routineGenerationService ?? throw new ArgumentNullException(nameof(routineGenerationService));
    }

    public async Task<WorkoutPlanGenerationResult> GenerateWorkoutPlanAsync(
        string userName,
        int age,
        string gender,
        string fitnessLevel,
        int trainingDays,
        List<string> goals,
        CancellationToken cancellationToken = default)
    {
        // Create UserProfile entity for legacy service
        var userProfile = new Entities.UserProfile
        {
            Name = userName,
            Age = age,
            Gender = gender,
            FitnessLevel = fitnessLevel,
            TrainingDays = trainingDays,
            Goals = goals
        };

        // Call legacy service
        var result = await _routineGenerationService.GeneratePersonalizedRoutineWithStructureAsync(userProfile);

        // Convert to Domain WorkoutPlan
        var workoutPlan = ConvertToWorkoutPlan(userProfile, result.workouts);
        var routineText = result.text ?? string.Empty;

        return new WorkoutPlanGenerationResult(workoutPlan, routineText);
    }

    public async Task<string> GenerateAlternativeRoutineTextAsync(
        string userName,
        int age,
        string gender,
        string fitnessLevel,
        int trainingDays,
        List<string> goals,
        CancellationToken cancellationToken = default)
    {
        // Create UserProfile entity for legacy service
        var userProfile = new Entities.UserProfile
        {
            Name = userName,
            Age = age,
            Gender = gender,
            FitnessLevel = fitnessLevel,
            TrainingDays = trainingDays,
            Goals = goals
        };

        // Call legacy service
        var routine = await _routineGenerationService.GenerateAlternativeRoutineAsync(userProfile);

        return routine;
    }

    public async Task<bool> IsAIAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await _routineGenerationService.IsAIAvailableAsync();
    }

    public async Task<string> GetAIStatusAsync(CancellationToken cancellationToken = default)
    {
        return await _routineGenerationService.GetAIStatusAsync();
    }

    private WorkoutPlan ConvertToWorkoutPlan(Entities.UserProfile userProfile, List<GymRoutineGenerator.Core.Models.WorkoutDay>? workoutDays)
    {
        var difficultyLevel = MapDifficultyLevel(userProfile.FitnessLevel);
        var totalDays = workoutDays?.Count ?? userProfile.TrainingDays;
        if (totalDays <= 0)
        {
            totalDays = 3;
        }

        totalDays = Math.Clamp(totalDays, 1, 7);

        var workoutPlan = WorkoutPlan.Create(
            name: $"Plan de Entrenamiento - {userProfile.Name}",
            userName: userProfile.Name,
            userAge: userProfile.Age > 0 ? userProfile.Age : 30,
            gender: string.IsNullOrWhiteSpace(userProfile.Gender) ? "No especificado" : userProfile.Gender,
            userLevel: difficultyLevel,
            trainingDaysPerWeek: totalDays,
            description: $"Plan personalizado de {totalDays} días"
        );

        if (workoutDays == null || workoutDays.Count == 0)
        {
            return workoutPlan;
        }

        var dayNumber = 1;
        foreach (var workoutDay in workoutDays)
        {
            if (dayNumber > workoutPlan.TrainingDaysPerWeek)
            {
                break;
            }

            var routine = Routine.Create(
                name: string.IsNullOrWhiteSpace(workoutDay.Name) ? $"Día {dayNumber}" : workoutDay.Name,
                dayNumber: workoutDay.DayNumber > 0 ? workoutDay.DayNumber : dayNumber,
                description: string.IsNullOrWhiteSpace(workoutDay.FocusAreas) ? workoutDay.Description : workoutDay.FocusAreas
            );

            var order = 1;
            foreach (var exercise in workoutDay.Exercises)
            {
                var equipment = MapEquipmentType(exercise.Equipment);
                var difficulty = MapDifficultyLevel(exercise.DifficultyLevel);
                var domainExercise = Exercise.Create(
                    name: string.IsNullOrWhiteSpace(exercise.Name) ? $"Ejercicio {order}" : exercise.Name,
                    equipment: equipment,
                    difficulty: difficulty,
                    description: string.IsNullOrWhiteSpace(exercise.Description) ? exercise.Instructions : exercise.Description
                );

                if (exercise.MuscleGroups?.Any() == true)
                {
                    foreach (var muscleName in exercise.MuscleGroups)
                    {
                        var muscle = MapMuscleGroup(muscleName);
                        if (muscle != null)
                        {
                            domainExercise.AddTargetMuscle(muscle);
                        }
                    }
                }

                var repetitions = ParseRepetitions(exercise.RecommendedReps);
                var totalSets = exercise.RecommendedSets > 0 ? exercise.RecommendedSets : 3;
                var sets = new List<Domain.ValueObjects.ExerciseSet>();
                for (var i = 0; i < totalSets; i++)
                {
                    sets.Add(Domain.ValueObjects.ExerciseSet.Create(repetitions));
                }

                routine.AddExercise(domainExercise, order++, sets);
            }

            workoutPlan.AddRoutine(routine);
            dayNumber++;
        }

        return workoutPlan;
    }

    private static Domain.ValueObjects.DifficultyLevel MapDifficultyLevel(string? level)
    {
        if (string.IsNullOrWhiteSpace(level))
        {
            return Domain.ValueObjects.DifficultyLevel.Intermedio;
        }

        return level.Trim().ToLowerInvariant() switch
        {
            "principiante" => Domain.ValueObjects.DifficultyLevel.Principiante,
            "principiante avanzado" => Domain.ValueObjects.DifficultyLevel.PrincipianteAvanzado,
            "intermedio" => Domain.ValueObjects.DifficultyLevel.Intermedio,
            "avanzado" => Domain.ValueObjects.DifficultyLevel.Avanzado,
            "experto" => Domain.ValueObjects.DifficultyLevel.Experto,
            _ => Domain.ValueObjects.DifficultyLevel.Intermedio
        };
    }

    private static Domain.ValueObjects.EquipmentType MapEquipmentType(string? equipment)
    {
        if (string.IsNullOrWhiteSpace(equipment))
        {
            return Domain.ValueObjects.EquipmentType.PesoCorporal;
        }

        var normalized = equipment.Trim().ToLowerInvariant();
        return normalized switch
        {
            "mancuernas" or "dumbbells" => Domain.ValueObjects.EquipmentType.Mancuernas,
            "barra" or "barbell" => Domain.ValueObjects.EquipmentType.Barra,
            "maquina" or "máquina" or "machine" => Domain.ValueObjects.EquipmentType.Maquina,
            "kettlebell" => Domain.ValueObjects.EquipmentType.Kettlebell,
            "banda elastica" or "banda elástica" or "resistance band" => Domain.ValueObjects.EquipmentType.BandaElastica,
            "polea" or "cable" or "cable machine" => Domain.ValueObjects.EquipmentType.Polea,
            "trx" => Domain.ValueObjects.EquipmentType.TRX,
            _ => Domain.ValueObjects.EquipmentType.PesoCorporal
        };
    }

    private static Domain.ValueObjects.MuscleGroup? MapMuscleGroup(string? muscle)
    {
        if (string.IsNullOrWhiteSpace(muscle))
        {
            return null;
        }

        var normalized = muscle.Trim().ToLowerInvariant();
        return normalized switch
        {
            "pecho" or "chest" => Domain.ValueObjects.MuscleGroup.Pecho,
            "espalda" or "back" => Domain.ValueObjects.MuscleGroup.Espalda,
            "hombro" or "hombros" or "shoulder" or "shoulders" => Domain.ValueObjects.MuscleGroup.Hombros,
            "bicep" or "bíceps" or "biceps" => Domain.ValueObjects.MuscleGroup.Biceps,
            "tricep" or "tríceps" or "triceps" => Domain.ValueObjects.MuscleGroup.Triceps,
            "cuadriceps" or "cuádriceps" or "quadriceps" => Domain.ValueObjects.MuscleGroup.Cuadriceps,
            "isquiotibiales" or "hamstrings" => Domain.ValueObjects.MuscleGroup.Isquiotibiales,
            "gluteos" or "glúteos" or "glutes" => Domain.ValueObjects.MuscleGroup.Gluteos,
            "pantorrillas" or "calves" => Domain.ValueObjects.MuscleGroup.Pantorrillas,
            "abdominales" or "abs" or "core" => Domain.ValueObjects.MuscleGroup.Abdominales,
            "lumbares" or "lower back" => Domain.ValueObjects.MuscleGroup.Lumbares,
            _ => null
        };
    }

    private static int ParseRepetitions(string? reps)
    {
        if (string.IsNullOrWhiteSpace(reps))
        {
            return 10;
        }

        var digits = new string(reps.Where(char.IsDigit).ToArray());
        if (int.TryParse(digits, out var value) && value > 0)
        {
            return value;
        }

        return 10;
    }
}
