using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Core.Enums;

namespace GymRoutineGenerator.Data.Seeds;

public static class EnhancedExerciseSeeder
{
    public static void SeedData(GymRoutineContext context)
    {
        // First ensure lookup tables are seeded
        MuscleGroupSeeder.SeedData(context);
        EquipmentTypeSeeder.SeedData(context);

        if (context.Exercises.Any())
            return; // Already seeded

        // Get lookup data
        var bodyweightEquipment = context.EquipmentTypes.First(e => e.Name == "Bodyweight");
        var freeWeightsEquipment = context.EquipmentTypes.First(e => e.Name == "Free Weights");
        var machinesEquipment = context.EquipmentTypes.First(e => e.Name == "Machines");

        var chestMuscle = context.MuscleGroups.First(m => m.Name == "Chest");
        var legsMuscle = context.MuscleGroups.First(m => m.Name == "Legs");
        var backMuscle = context.MuscleGroups.First(m => m.Name == "Back");
        var coreMuscle = context.MuscleGroups.First(m => m.Name == "Core");
        var shouldersMuscle = context.MuscleGroups.First(m => m.Name == "Shoulders");
        var armsMuscle = context.MuscleGroups.First(m => m.Name == "Arms");
        var glutesMuscle = context.MuscleGroups.First(m => m.Name == "Glutes");
        var fullBodyMuscle = context.MuscleGroups.First(m => m.Name == "FullBody");

        var exercises = new List<Exercise>
        {
            new Exercise
            {
                Name = "Push-ups",
                SpanishName = "Flexiones de Pecho",
                Description = "Ejercicio básico de empuje para fortalecer el pecho, hombros y tríceps",
                Instructions = "Colócate en posición de plancha. Baja el cuerpo hasta que el pecho casi toque el suelo. Empuja hacia arriba hasta la posición inicial.",
                PrimaryMuscleGroupId = chestMuscle.Id,
                EquipmentTypeId = bodyweightEquipment.Id,
                DifficultyLevel = DifficultyLevel.Beginner,
                ExerciseType = ExerciseType.Strength,
                DurationSeconds = null
            },
            new Exercise
            {
                Name = "Squats",
                SpanishName = "Sentadillas",
                Description = "Ejercicio fundamental para piernas y glúteos",
                Instructions = "Ponte de pie con los pies separados al ancho de los hombros. Baja como si fueras a sentarte en una silla. Regresa a la posición inicial.",
                PrimaryMuscleGroupId = legsMuscle.Id,
                EquipmentTypeId = bodyweightEquipment.Id,
                DifficultyLevel = DifficultyLevel.Beginner,
                ExerciseType = ExerciseType.Strength,
                DurationSeconds = null
            },
            new Exercise
            {
                Name = "Pull-ups",
                SpanishName = "Dominadas",
                Description = "Ejercicio de tracción para desarrollar la espalda y bíceps",
                Instructions = "Cuelgate de una barra con las palmas hacia adelante. Tira de tu cuerpo hacia arriba hasta que la barbilla pase la barra.",
                PrimaryMuscleGroupId = backMuscle.Id,
                EquipmentTypeId = context.EquipmentTypes.First(e => e.Name == "Pull-up Bar").Id,
                DifficultyLevel = DifficultyLevel.Intermediate,
                ExerciseType = ExerciseType.Strength,
                DurationSeconds = null
            },
            new Exercise
            {
                Name = "Plank",
                SpanishName = "Plancha",
                Description = "Ejercicio isométrico para fortalecer el core",
                Instructions = "Mantén una posición de plancha con el cuerpo recto desde la cabeza hasta los talones. Contrae el abdomen.",
                PrimaryMuscleGroupId = coreMuscle.Id,
                EquipmentTypeId = bodyweightEquipment.Id,
                DifficultyLevel = DifficultyLevel.Beginner,
                ExerciseType = ExerciseType.Strength,
                DurationSeconds = 30
            },
            new Exercise
            {
                Name = "Bench Press",
                SpanishName = "Press de Banca",
                Description = "Ejercicio básico de empuje con barra para el pecho",
                Instructions = "Acuéstate en el banco. Baja la barra hasta el pecho controladamente. Empuja hacia arriba hasta extender los brazos.",
                PrimaryMuscleGroupId = chestMuscle.Id,
                EquipmentTypeId = freeWeightsEquipment.Id,
                DifficultyLevel = DifficultyLevel.Intermediate,
                ExerciseType = ExerciseType.Strength,
                DurationSeconds = null
            },
            new Exercise
            {
                Name = "Deadlift",
                SpanishName = "Peso Muerto",
                Description = "Ejercicio compuesto para espalda, piernas y glúteos",
                Instructions = "Con la barra en el suelo, inclínate y agárrala. Levanta manteniendo la espalda recta hasta estar de pie completamente.",
                PrimaryMuscleGroupId = backMuscle.Id,
                EquipmentTypeId = freeWeightsEquipment.Id,
                DifficultyLevel = DifficultyLevel.Advanced,
                ExerciseType = ExerciseType.Strength,
                DurationSeconds = null
            },
            new Exercise
            {
                Name = "Military Press",
                SpanishName = "Press Militar",
                Description = "Ejercicio de empuje vertical para hombros",
                Instructions = "De pie, levanta la barra desde los hombros hasta arriba de la cabeza. Baja controladamente.",
                PrimaryMuscleGroupId = shouldersMuscle.Id,
                EquipmentTypeId = freeWeightsEquipment.Id,
                DifficultyLevel = DifficultyLevel.Intermediate,
                ExerciseType = ExerciseType.Strength,
                DurationSeconds = null
            },
            new Exercise
            {
                Name = "Bicep Curls",
                SpanishName = "Curl de Bíceps",
                Description = "Ejercicio de aislamiento para bíceps",
                Instructions = "Con mancuernas en las manos, flexiona los codos llevando el peso hacia los hombros. Baja controladamente.",
                PrimaryMuscleGroupId = armsMuscle.Id,
                EquipmentTypeId = freeWeightsEquipment.Id,
                DifficultyLevel = DifficultyLevel.Beginner,
                ExerciseType = ExerciseType.Strength,
                DurationSeconds = null
            },
            new Exercise
            {
                Name = "Tricep Pushdown",
                SpanishName = "Tríceps en Polea",
                Description = "Ejercicio de aislamiento para tríceps en máquina",
                Instructions = "En la polea alta, empuja la barra hacia abajo extendiendo los codos. Regresa controladamente.",
                PrimaryMuscleGroupId = armsMuscle.Id,
                EquipmentTypeId = machinesEquipment.Id,
                DifficultyLevel = DifficultyLevel.Beginner,
                ExerciseType = ExerciseType.Strength,
                DurationSeconds = null
            },
            new Exercise
            {
                Name = "Lunges",
                SpanishName = "Zancadas",
                Description = "Ejercicio unilateral para piernas y glúteos",
                Instructions = "Da un paso largo hacia adelante. Baja la rodilla trasera hacia el suelo. Regresa a la posición inicial.",
                PrimaryMuscleGroupId = legsMuscle.Id,
                EquipmentTypeId = bodyweightEquipment.Id,
                DifficultyLevel = DifficultyLevel.Beginner,
                ExerciseType = ExerciseType.Strength,
                DurationSeconds = null
            },
            new Exercise
            {
                Name = "Lateral Raises",
                SpanishName = "Elevaciones Laterales",
                Description = "Ejercicio de aislamiento para deltoides medio",
                Instructions = "Con mancuernas a los lados, levanta los brazos hacia los costados hasta la altura de los hombros.",
                PrimaryMuscleGroupId = shouldersMuscle.Id,
                EquipmentTypeId = freeWeightsEquipment.Id,
                DifficultyLevel = DifficultyLevel.Beginner,
                ExerciseType = ExerciseType.Strength,
                DurationSeconds = null
            },
            new Exercise
            {
                Name = "Glute Bridge",
                SpanishName = "Puentes de Glúteo",
                Description = "Ejercicio específico para activar y fortalecer los glúteos",
                Instructions = "Acostado boca arriba, levanta las caderas contrayendo los glúteos. Mantén y baja controladamente.",
                PrimaryMuscleGroupId = glutesMuscle.Id,
                EquipmentTypeId = bodyweightEquipment.Id,
                DifficultyLevel = DifficultyLevel.Beginner,
                ExerciseType = ExerciseType.Strength,
                DurationSeconds = null
            },
            new Exercise
            {
                Name = "Dumbbell Row",
                SpanishName = "Remo con Mancuernas",
                Description = "Ejercicio de tracción para espalda media",
                Instructions = "Con apoyo en banco, rema la mancuerna hacia el costado del torso. Contrae el omóplato.",
                PrimaryMuscleGroupId = backMuscle.Id,
                EquipmentTypeId = freeWeightsEquipment.Id,
                DifficultyLevel = DifficultyLevel.Beginner,
                ExerciseType = ExerciseType.Strength,
                DurationSeconds = null
            },
            new Exercise
            {
                Name = "Crunches",
                SpanishName = "Abdominales Crunch",
                Description = "Ejercicio básico para abdominales superiores",
                Instructions = "Acostado boca arriba, levanta los hombros del suelo contrayendo el abdomen. Baja controladamente.",
                PrimaryMuscleGroupId = coreMuscle.Id,
                EquipmentTypeId = bodyweightEquipment.Id,
                DifficultyLevel = DifficultyLevel.Beginner,
                ExerciseType = ExerciseType.Strength,
                DurationSeconds = null
            },
            new Exercise
            {
                Name = "Burpees",
                SpanishName = "Burpees",
                Description = "Ejercicio de cuerpo completo que combina fuerza y cardio",
                Instructions = "Desde de pie, baja a posición de flexión, haz una flexión, salta los pies hacia las manos y salta arriba.",
                PrimaryMuscleGroupId = fullBodyMuscle.Id,
                EquipmentTypeId = bodyweightEquipment.Id,
                DifficultyLevel = DifficultyLevel.Intermediate,
                ExerciseType = ExerciseType.Plyometric,
                DurationSeconds = null
            }
        };

        context.Exercises.AddRange(exercises);
        context.SaveChanges();
    }
}