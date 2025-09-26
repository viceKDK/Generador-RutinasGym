using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Data.Seeds;

public static class MuscleGroupSeeder
{
    public static void SeedData(GymRoutineContext context)
    {
        if (context.MuscleGroups.Any())
            return; // Already seeded

        var muscleGroups = new List<MuscleGroup>
        {
            new MuscleGroup
            {
                Name = "Chest",
                SpanishName = "Pecho",
                Description = "Pectorales mayor y menor"
            },
            new MuscleGroup
            {
                Name = "Back",
                SpanishName = "Espalda",
                Description = "Latissimus dorsi, rhomboides, trapecio"
            },
            new MuscleGroup
            {
                Name = "Shoulders",
                SpanishName = "Hombros",
                Description = "Deltoides anterior, medio y posterior"
            },
            new MuscleGroup
            {
                Name = "Arms",
                SpanishName = "Brazos",
                Description = "Bíceps, tríceps, antebrazos"
            },
            new MuscleGroup
            {
                Name = "Legs",
                SpanishName = "Piernas",
                Description = "Cuadriceps, isquiotibiales, pantorrillas"
            },
            new MuscleGroup
            {
                Name = "Core",
                SpanishName = "Core",
                Description = "Abdominales, oblicuos, transverso del abdomen"
            },
            new MuscleGroup
            {
                Name = "Glutes",
                SpanishName = "Glúteos",
                Description = "Glúteo mayor, medio y menor"
            },
            new MuscleGroup
            {
                Name = "FullBody",
                SpanishName = "Cuerpo Completo",
                Description = "Ejercicios que involucran múltiples grupos musculares"
            }
        };

        context.MuscleGroups.AddRange(muscleGroups);
        context.SaveChanges();
    }
}