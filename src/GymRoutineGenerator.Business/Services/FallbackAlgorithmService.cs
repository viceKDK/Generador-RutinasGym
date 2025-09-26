using System.Text;
using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Data.Repositories;
using CoreGender = GymRoutineGenerator.Core.Enums.Gender;
using CoreEquipmentType = GymRoutineGenerator.Core.Enums.EquipmentType;
using CoreMuscleGroup = GymRoutineGenerator.Core.Enums.MuscleGroup;

namespace GymRoutineGenerator.Business.Services;

public class FallbackAlgorithmService : IFallbackAlgorithmService
{
    private readonly IExerciseRepository _exerciseRepository;

    public FallbackAlgorithmService(IExerciseRepository exerciseRepository)
    {
        _exerciseRepository = exerciseRepository;
    }

    public async Task<string> GenerateBasicRoutineAsync(CoreGender gender, int age, int trainingDays, List<CoreEquipmentType> equipment)
    {
        var exercises = await _exerciseRepository.GetAllAsync();
        var availableExercises = exercises.Where(e => e.EquipmentType != null && HasRequiredEquipment(e, equipment)).ToList();

        if (!availableExercises.Any())
        {
            availableExercises = exercises.Where(e => e.EquipmentType != null && e.EquipmentType.Name.ToLower().Contains("peso corporal")).ToList();
        }

        var routine = new StringBuilder();
        routine.AppendLine($"🏋️ Rutina Básica Generada - {trainingDays} días por semana");
        routine.AppendLine($"👤 Perfil: {GetGenderText(gender)}, {age} años");
        routine.AppendLine();

        var selectedExercises = SelectExercisesForRoutine(availableExercises, trainingDays);

        for (int day = 1; day <= trainingDays; day++)
        {
            routine.AppendLine($"📅 Día {day}:");

            var dayExercises = GetExercisesForDay(selectedExercises, day, trainingDays);

            foreach (var exercise in dayExercises)
            {
                var sets = GetRecommendedSets(age, GetMuscleGroupFromExercise(exercise));
                var reps = GetRecommendedReps(age, gender, GetMuscleGroupFromExercise(exercise));

                routine.AppendLine($"  • {exercise.Name} - {sets} series x {reps} repeticiones");
            }

            routine.AppendLine();
        }

        routine.AppendLine("💡 Consejos:");
        routine.AppendLine("  - Descansa 48-72 horas entre entrenamientos del mismo grupo muscular");
        routine.AppendLine("  - Mantén buena forma en todos los ejercicios");
        routine.AppendLine("  - Aumenta gradualmente la intensidad");

        return routine.ToString();
    }

    private List<Exercise> SelectExercisesForRoutine(List<Exercise> exercises, int trainingDays)
    {
        var selectedExercises = new List<Exercise>();

        // Prioritize compound movements and major muscle groups
        var priorityGroups = new List<CoreMuscleGroup>
        {
            CoreMuscleGroup.Legs,
            CoreMuscleGroup.Chest,
            CoreMuscleGroup.Back,
            CoreMuscleGroup.Shoulders,
            CoreMuscleGroup.Arms,
            CoreMuscleGroup.Core
        };

        foreach (var group in priorityGroups)
        {
            var groupExercises = exercises.Where(e => GetMuscleGroupFromExercise(e) == group).ToList();
            if (groupExercises.Any())
            {
                // Select 1-2 exercises per muscle group
                selectedExercises.AddRange(groupExercises.Take(trainingDays >= 4 ? 2 : 1));
            }
        }

        // Add full body if training days are low
        if (trainingDays <= 2)
        {
            var fullBodyExercises = exercises.Where(e => GetMuscleGroupFromExercise(e) == CoreMuscleGroup.Core).ToList();
            selectedExercises.AddRange(fullBodyExercises.Take(2));
        }

        return selectedExercises.Take(12).ToList(); // Limit total exercises
    }

    private List<Exercise> GetExercisesForDay(List<Exercise> exercises, int day, int totalDays)
    {
        if (totalDays <= 2)
        {
            // Full body workouts
            return exercises.Take(6).ToList();
        }
        else if (totalDays == 3)
        {
            // Upper/Lower/Push-Pull split
            return day switch
            {
                1 => exercises.Where(e => GetMuscleGroupFromExercise(e) == CoreMuscleGroup.Chest ||
                                         GetMuscleGroupFromExercise(e) == CoreMuscleGroup.Shoulders ||
                                         GetMuscleGroupFromExercise(e) == CoreMuscleGroup.Arms).Take(4).ToList(),
                2 => exercises.Where(e => GetMuscleGroupFromExercise(e) == CoreMuscleGroup.Legs ||
                                         GetMuscleGroupFromExercise(e) == CoreMuscleGroup.Core).Take(4).ToList(),
                3 => exercises.Where(e => GetMuscleGroupFromExercise(e) == CoreMuscleGroup.Back ||
                                         GetMuscleGroupFromExercise(e) == CoreMuscleGroup.Arms ||
                                         GetMuscleGroupFromExercise(e) == CoreMuscleGroup.Core).Take(4).ToList(),
                _ => exercises.Take(4).ToList()
            };
        }
        else
        {
            // Body part split
            var muscleGroups = new[] { CoreMuscleGroup.Chest, CoreMuscleGroup.Back, CoreMuscleGroup.Legs,
                                     CoreMuscleGroup.Shoulders, CoreMuscleGroup.Arms, CoreMuscleGroup.Core };
            var targetGroup = muscleGroups[(day - 1) % muscleGroups.Length];

            var targetExercises = exercises.Where(e => GetMuscleGroupFromExercise(e) == targetGroup).ToList();
            return targetExercises.Any() ? targetExercises.Take(3).ToList() : exercises.Take(3).ToList();
        }
    }

    private int GetRecommendedSets(int age, CoreMuscleGroup muscleGroup)
    {
        // Adjust sets based on age and muscle group
        var baseSets = muscleGroup switch
        {
            CoreMuscleGroup.Legs or CoreMuscleGroup.Back or CoreMuscleGroup.Chest => 3,
            CoreMuscleGroup.Shoulders or CoreMuscleGroup.Arms => 3,
            CoreMuscleGroup.Core => 2,
            _ => 3
        };

        // Reduce volume for older adults
        if (age > 50) baseSets = Math.Max(2, baseSets - 1);
        if (age > 65) baseSets = 2;

        return baseSets;
    }

    private string GetRecommendedReps(int age, CoreGender gender, CoreMuscleGroup muscleGroup)
    {
        // Base rep ranges
        var baseReps = muscleGroup switch
        {
            CoreMuscleGroup.Legs => "12-15",
            CoreMuscleGroup.Chest or CoreMuscleGroup.Back => "8-12",
            CoreMuscleGroup.Shoulders or CoreMuscleGroup.Arms => "10-15",
            CoreMuscleGroup.Core => "15-20",
            _ => "10-12"
        };

        // Adjust for age
        if (age > 50)
        {
            return muscleGroup switch
            {
                CoreMuscleGroup.Legs => "10-12",
                CoreMuscleGroup.Core => "12-15",
                _ => "8-10"
            };
        }

        return baseReps;
    }

    private string GetGenderText(CoreGender gender)
    {
        return gender switch
        {
            CoreGender.Male => "Hombre",
            CoreGender.Female => "Mujer",
            CoreGender.Other => "Otro",
            _ => "No especificado"
        };
    }

    private bool HasRequiredEquipment(Exercise exercise, List<CoreEquipmentType> availableEquipment)
    {
        if (exercise.EquipmentType == null) return false;

        var exerciseEquipment = exercise.EquipmentType.Name.ToLower();

        return availableEquipment.Any(eq => eq switch
        {
            CoreEquipmentType.FreeWeights => exerciseEquipment.Contains("barra") || exerciseEquipment.Contains("mancuerna") || exerciseEquipment.Contains("peso libre"),
            CoreEquipmentType.Machines => exerciseEquipment.Contains("máquina"),
            CoreEquipmentType.Bodyweight => exerciseEquipment.Contains("peso corporal") || exerciseEquipment.Contains("cuerpo"),
            CoreEquipmentType.ResistanceBands => exerciseEquipment.Contains("banda") || exerciseEquipment.Contains("elástica"),
            CoreEquipmentType.Other => true,
            _ => false
        });
    }

    private CoreMuscleGroup GetMuscleGroupFromExercise(Exercise exercise)
    {
        if (exercise.PrimaryMuscleGroup == null) return CoreMuscleGroup.Core;

        var muscleName = exercise.PrimaryMuscleGroup.Name.ToLower();

        return muscleName switch
        {
            var name when name.Contains("pecho") => CoreMuscleGroup.Chest,
            var name when name.Contains("espalda") => CoreMuscleGroup.Back,
            var name when name.Contains("pierna") || name.Contains("cuádriceps") || name.Contains("glúteo") || name.Contains("gemelo") => CoreMuscleGroup.Legs,
            var name when name.Contains("hombro") => CoreMuscleGroup.Shoulders,
            var name when name.Contains("brazo") || name.Contains("bíceps") || name.Contains("tríceps") => CoreMuscleGroup.Arms,
            var name when name.Contains("core") || name.Contains("abdominal") => CoreMuscleGroup.Core,
            _ => CoreMuscleGroup.Core
        };
    }
}