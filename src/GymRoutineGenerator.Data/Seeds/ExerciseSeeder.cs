using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Core.Enums;

namespace GymRoutineGenerator.Data.Seeds;

public static class ExerciseSeeder
{
    // DEPRECATED: Use EnhancedExerciseSeeder instead
    /*
    public static void SeedData(GymRoutineContext context)
    {
        if (context.Exercises.Any())
            return; // Already seeded

        var exercises = new List<Exercise>
        {
            new Exercise
            {
                Name = "Flexiones de Pecho",
                MuscleGroup = MuscleGroup.Chest,
                Equipment = EquipmentType.Bodyweight,
                ImagePath = "images/push_ups.jpg"
            },
            new Exercise
            {
                Name = "Sentadillas",
                MuscleGroup = MuscleGroup.Legs,
                Equipment = EquipmentType.Bodyweight,
                ImagePath = "images/squats.jpg"
            },
            new Exercise
            {
                Name = "Dominadas",
                MuscleGroup = MuscleGroup.Back,
                Equipment = EquipmentType.FreeWeights,
                ImagePath = "images/pull_ups.jpg"
            },
            new Exercise
            {
                Name = "Plancha",
                MuscleGroup = MuscleGroup.Core,
                Equipment = EquipmentType.Bodyweight,
                ImagePath = "images/plank.jpg"
            },
            new Exercise
            {
                Name = "Press de Banca",
                MuscleGroup = MuscleGroup.Chest,
                Equipment = EquipmentType.FreeWeights,
                ImagePath = "images/bench_press.jpg"
            },
            new Exercise
            {
                Name = "Peso Muerto",
                MuscleGroup = MuscleGroup.Back,
                Equipment = EquipmentType.FreeWeights,
                ImagePath = "images/deadlift.jpg"
            },
            new Exercise
            {
                Name = "Press Militar",
                MuscleGroup = MuscleGroup.Shoulders,
                Equipment = EquipmentType.FreeWeights,
                ImagePath = "images/military_press.jpg"
            },
            new Exercise
            {
                Name = "Curl de Bíceps",
                MuscleGroup = MuscleGroup.Arms,
                Equipment = EquipmentType.FreeWeights,
                ImagePath = "images/bicep_curl.jpg"
            },
            new Exercise
            {
                Name = "Tríceps en Polea",
                MuscleGroup = MuscleGroup.Arms,
                Equipment = EquipmentType.Machines,
                ImagePath = "images/tricep_pushdown.jpg"
            },
            new Exercise
            {
                Name = "Zancadas",
                MuscleGroup = MuscleGroup.Legs,
                Equipment = EquipmentType.Bodyweight,
                ImagePath = "images/lunges.jpg"
            },
            new Exercise
            {
                Name = "Elevaciones Laterales",
                MuscleGroup = MuscleGroup.Shoulders,
                Equipment = EquipmentType.FreeWeights,
                ImagePath = "images/lateral_raises.jpg"
            },
            new Exercise
            {
                Name = "Puentes de Glúteo",
                MuscleGroup = MuscleGroup.Glutes,
                Equipment = EquipmentType.Bodyweight,
                ImagePath = "images/glute_bridge.jpg"
            },
            new Exercise
            {
                Name = "Remo con Mancuernas",
                MuscleGroup = MuscleGroup.Back,
                Equipment = EquipmentType.FreeWeights,
                ImagePath = "images/dumbbell_row.jpg"
            },
            new Exercise
            {
                Name = "Abdominales Crunch",
                MuscleGroup = MuscleGroup.Core,
                Equipment = EquipmentType.Bodyweight,
                ImagePath = "images/crunches.jpg"
            },
            new Exercise
            {
                Name = "Burpees",
                MuscleGroup = MuscleGroup.FullBody,
                Equipment = EquipmentType.Bodyweight,
                ImagePath = "images/burpees.jpg"
            }
        };

        context.Exercises.AddRange(exercises);
        context.SaveChanges();
    }
    */
}