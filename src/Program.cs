using System;
using System.Threading.Tasks;
using GymRoutineGenerator.Core.Models.Routines;
using GymRoutineGenerator.Core.Services.Documents;
using GymRoutineGenerator.Infrastructure.Documents;

namespace GymRoutineGenerator.Console;

class Program
{
    static async Task Main(string[] args)
    {
        System.Console.WriteLine("🏋️ GENERADOR DE RUTINAS DE GIMNASIO");
        System.Console.WriteLine("==================================");
        System.Console.WriteLine("");

        try
        {
            // Inicializar servicios
            var wordService = new WordDocumentService();
            var templateService = new TemplateManagerService();
            var exportService = new SimpleExportService(wordService, templateService);

            // Crear una rutina de ejemplo
            var routine = CreateExampleRoutine();

            System.Console.WriteLine($"📋 Rutina creada: {routine.Name}");
            System.Console.WriteLine($"👤 Cliente: {routine.ClientName}");
            System.Console.WriteLine($"🎯 Objetivo: {routine.Goal}");
            System.Console.WriteLine($"📅 Duración: {routine.DurationWeeks} semanas");
            System.Console.WriteLine($"💪 Días de entrenamiento: {routine.Days.Count}");
            System.Console.WriteLine("");

            // Configurar exportación
            var outputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var options = new ExportOptions
            {
                OutputPath = outputPath,
                AutoOpenAfterExport = false,
                OverwriteExisting = true,
                CreateBackup = false
            };

            System.Console.WriteLine("📄 Exportando rutina a documento Word...");

            // Exportar con progreso
            var progress = new Progress<ExportProgress>(p =>
            {
                System.Console.Write($"\r⏳ {p.CurrentOperation} ({p.PercentComplete}%)");
            });

            var result = await exportService.ExportRoutineToWordAsync(routine, "professional", options, progress);

            System.Console.WriteLine(""); // Nueva línea después del progreso

            if (result.Success)
            {
                System.Console.WriteLine("");
                System.Console.WriteLine("🎉 ¡RUTINA EXPORTADA EXITOSAMENTE!");
                System.Console.WriteLine($"📁 Archivo: {result.FilePath}");
                System.Console.WriteLine($"📏 Tamaño: {result.FileSizeBytes / 1024:N0} KB");
                System.Console.WriteLine($"💪 Ejercicios incluidos: {result.ExerciseCount}");
                System.Console.WriteLine($"⏱️ Tiempo de generación: {result.ExportDuration.TotalSeconds:F1} segundos");
                System.Console.WriteLine("");
                System.Console.WriteLine("✅ La aplicación funciona correctamente!");
                System.Console.WriteLine("📂 Puedes encontrar el archivo en tu carpeta Documentos");
            }
            else
            {
                System.Console.WriteLine("");
                System.Console.WriteLine("❌ ERROR AL EXPORTAR:");
                System.Console.WriteLine($"   {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine("");
            System.Console.WriteLine("💥 ERROR INESPERADO:");
            System.Console.WriteLine($"   {ex.Message}");

            if (ex.InnerException != null)
            {
                System.Console.WriteLine($"   Detalles: {ex.InnerException.Message}");
            }
        }

        System.Console.WriteLine("");
        System.Console.WriteLine("Presiona cualquier tecla para salir...");
        System.Console.ReadKey();
    }

    static Routine CreateExampleRoutine()
    {
        var routine = new Routine
        {
            Id = 1,
            Name = "Rutina de Demostración",
            ClientName = "Usuario Demo",
            Description = "Rutina completa para demostrar la funcionalidad de exportación de documentos Word",
            Goal = "Demostración del sistema",
            DurationWeeks = 4,
            CreatedDate = DateTime.Now
        };

        // Día 1 - Pecho y Tríceps
        var day1 = new RoutineDay
        {
            Id = 1,
            DayNumber = 1,
            Name = "Día 1 - Pecho y Tríceps",
            Description = "Entrenamiento enfocado en músculos del pecho y tríceps",
            FocusArea = "Tren Superior",
            TargetIntensity = "Alta",
            EstimatedDurationMinutes = 75
        };

        day1.Exercises.Add(new RoutineExercise
        {
            Id = 1,
            Order = 1,
            Name = "Press de Banca",
            Category = "Fuerza",
            MuscleGroups = new List<string> { "Pectorales", "Tríceps", "Deltoides" },
            Equipment = "Barra y Banco",
            Instructions = "Acuéstese en el banco con los pies firmes en el suelo. Tome la barra con un agarre ligeramente más ancho que los hombros. Baje la barra hasta el pecho y empuje hacia arriba.",
            SafetyTips = "Mantenga los omóplatos retraídos. No rebote la barra en el pecho. Use un spotter para pesos pesados.",
            RestTimeSeconds = 90,
            Difficulty = "Intermedio",
            Sets = new List<ExerciseSet>
            {
                new ExerciseSet { Id = 1, SetNumber = 1, Reps = 12, Weight = 60, RestSeconds = 90 },
                new ExerciseSet { Id = 2, SetNumber = 2, Reps = 10, Weight = 70, RestSeconds = 90 },
                new ExerciseSet { Id = 3, SetNumber = 3, Reps = 8, Weight = 80, RestSeconds = 120 },
                new ExerciseSet { Id = 4, SetNumber = 4, Reps = 6, Weight = 85, RestSeconds = 120 }
            }
        });

        day1.Exercises.Add(new RoutineExercise
        {
            Id = 2,
            Order = 2,
            Name = "Press Inclinado con Mancuernas",
            Category = "Fuerza",
            MuscleGroups = new List<string> { "Pectorales Superior", "Deltoides" },
            Equipment = "Mancuernas y Banco Inclinado",
            Instructions = "Ajuste el banco a 30-45°. Tome las mancuernas y realice el movimiento de press desde el pecho hasta arriba.",
            SafetyTips = "Controle el descenso. No deje caer las mancuernas. Mantenga los pies firmes.",
            RestTimeSeconds = 75,
            Difficulty = "Intermedio",
            Sets = new List<ExerciseSet>
            {
                new ExerciseSet { Id = 5, SetNumber = 1, Reps = 12, Weight = 25, RestSeconds = 75 },
                new ExerciseSet { Id = 6, SetNumber = 2, Reps = 10, Weight = 30, RestSeconds = 75 },
                new ExerciseSet { Id = 7, SetNumber = 3, Reps = 8, Weight = 32.5m, RestSeconds = 90 }
            }
        });

        routine.Days.Add(day1);

        // Día 2 - Espalda y Bíceps
        var day2 = new RoutineDay
        {
            Id = 2,
            DayNumber = 2,
            Name = "Día 2 - Espalda y Bíceps",
            Description = "Entrenamiento enfocado en músculos de la espalda y bíceps",
            FocusArea = "Tren Superior",
            TargetIntensity = "Alta",
            EstimatedDurationMinutes = 80
        };

        day2.Exercises.Add(new RoutineExercise
        {
            Id = 3,
            Order = 1,
            Name = "Dominadas",
            Category = "Fuerza",
            MuscleGroups = new List<string> { "Dorsales", "Bíceps", "Romboides" },
            Equipment = "Barra de Dominadas",
            Instructions = "Cuelgue de la barra con agarre pronador. Tire del cuerpo hacia arriba hasta que la barbilla pase la barra. Descienda de forma controlada.",
            SafetyTips = "Controle el descenso. Si no puede hacer dominadas completas, use banda elástica o máquina asistida.",
            RestTimeSeconds = 90,
            Difficulty = "Avanzado",
            Sets = new List<ExerciseSet>
            {
                new ExerciseSet { Id = 8, SetNumber = 1, Reps = 8, Weight = 0, RestSeconds = 90 },
                new ExerciseSet { Id = 9, SetNumber = 2, Reps = 6, Weight = 0, RestSeconds = 90 },
                new ExerciseSet { Id = 10, SetNumber = 3, Reps = 5, Weight = 0, RestSeconds = 120 }
            }
        });

        routine.Days.Add(day2);

        // Calcular métricas
        routine.Metrics = new RoutineMetrics
        {
            TotalExercises = routine.Days.SelectMany(d => d.Exercises).Count(),
            TotalSets = routine.Days.SelectMany(d => d.Exercises).SelectMany(e => e.Sets).Count(),
            EstimatedDurationMinutes = routine.Days.Sum(d => d.EstimatedDurationMinutes),
            MuscleGroupsCovered = routine.Days.SelectMany(d => d.Exercises).SelectMany(e => e.MuscleGroups).Distinct().ToList(),
            EquipmentRequired = routine.Days.SelectMany(d => d.Exercises).Select(e => e.Equipment).Distinct().ToList(),
            DifficultyLevel = "Intermedio-Avanzado",
            CaloriesBurnedEstimate = 350
        };

        return routine;
    }
}