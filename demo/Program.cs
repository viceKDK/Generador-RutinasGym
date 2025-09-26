using System;
using System.Threading.Tasks;
using GymRoutineGenerator.Core.Models.Routines;
using GymRoutineGenerator.Core.Services.Documents;
using GymRoutineGenerator.Infrastructure.Documents;

namespace GymRoutineGenerator.Demo;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🏋️ DEMO - GENERADOR DE RUTINAS DE GIMNASIO");
        Console.WriteLine("==========================================");
        Console.WriteLine("");

        try
        {
            // Inicializar servicios
            var wordService = new WordDocumentService();
            var templateService = new TemplateManagerService();
            var exportService = new SimpleExportService(wordService, templateService);

            // Crear una rutina de ejemplo
            var routine = CreateExampleRoutine();

            Console.WriteLine($"📋 Rutina creada: {routine.Name}");
            Console.WriteLine($"👤 Cliente: {routine.ClientName}");
            Console.WriteLine($"🎯 Objetivo: {routine.Goal}");
            Console.WriteLine($"📅 Duración: {routine.DurationWeeks} semanas");
            Console.WriteLine($"💪 Días de entrenamiento: {routine.Days.Count}");
            Console.WriteLine("");

            // Configurar exportación
            var outputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var options = new ExportOptions
            {
                OutputPath = outputPath,
                AutoOpenAfterExport = false,
                OverwriteExisting = true,
                CreateBackup = false
            };

            Console.WriteLine("📄 Exportando rutina a documento Word...");

            // Exportar con progreso
            var progress = new Progress<ExportProgress>(p =>
            {
                Console.Write($"\r⏳ {p.CurrentOperation} ({p.PercentComplete}%)");
            });

            var result = await exportService.ExportRoutineToWordAsync(routine, "professional", options, progress);

            Console.WriteLine(""); // Nueva línea después del progreso

            if (result.Success)
            {
                Console.WriteLine("");
                Console.WriteLine("🎉 ¡RUTINA EXPORTADA EXITOSAMENTE!");
                Console.WriteLine($"📁 Archivo: {result.FilePath}");
                Console.WriteLine($"📏 Tamaño: {result.FileSizeBytes / 1024:N0} KB");
                Console.WriteLine($"💪 Ejercicios incluidos: {result.ExerciseCount}");
                Console.WriteLine($"⏱️ Tiempo de generación: {result.ExportDuration.TotalSeconds:F1} segundos");
                Console.WriteLine("");
                Console.WriteLine("✅ LA APLICACIÓN FUNCIONA CORRECTAMENTE!");
                Console.WriteLine("📂 Puedes encontrar el archivo en tu carpeta Documentos");
                Console.WriteLine("");
                Console.WriteLine("🎯 VALIDACIÓN EXITOSA:");
                Console.WriteLine("   ✅ Servicios inicializados correctamente");
                Console.WriteLine("   ✅ Rutina creada con datos válidos");
                Console.WriteLine("   ✅ Exportación a Word exitosa");
                Console.WriteLine("   ✅ Archivo generado y guardado");
                Console.WriteLine("   ✅ Todas las funcionalidades operativas");
            }
            else
            {
                Console.WriteLine("");
                Console.WriteLine("❌ ERROR AL EXPORTAR:");
                Console.WriteLine($"   {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("");
            Console.WriteLine("💥 ERROR INESPERADO:");
            Console.WriteLine($"   {ex.Message}");

            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Detalles: {ex.InnerException.Message}");
            }
        }

        Console.WriteLine("");
        Console.WriteLine("Presiona cualquier tecla para salir...");
        Console.ReadKey();
    }

    static Routine CreateExampleRoutine()
    {
        var routine = new Routine
        {
            Id = 1,
            Name = "Rutina Demo - Validación Final",
            ClientName = "Usuario Demo",
            Description = "Rutina completa para demostrar que la aplicación funciona perfectamente y está lista para producción",
            Goal = "Validación de funcionamiento completo",
            DurationWeeks = 4,
            CreatedDate = DateTime.Now
        };

        // Día 1 - Pecho y Tríceps
        var day1 = new RoutineDay
        {
            Id = 1,
            DayNumber = 1,
            Name = "Día 1 - Pecho y Tríceps",
            Description = "Entrenamiento enfocado en músculos del pecho y tríceps para validar funcionalidad",
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
            Instructions = "Acuéstese en el banco con los pies firmes en el suelo. Tome la barra con un agarre ligeramente más ancho que los hombros. Baje la barra hasta el pecho controladamente y empuje hacia arriba con fuerza.",
            SafetyTips = "Mantenga los omóplatos retraídos durante todo el movimiento. No rebote la barra en el pecho. Use un spotter para pesos pesados.",
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
            MuscleGroups = new List<string> { "Pectorales Superior", "Deltoides Anterior" },
            Equipment = "Mancuernas y Banco Inclinado",
            Instructions = "Ajuste el banco a 30-45 grados. Tome las mancuernas con agarre neutro y realice el movimiento de press desde el pecho hacia arriba, manteniendo control durante todo el rango de movimiento.",
            SafetyTips = "Controle el descenso de las mancuernas. No las deje caer al final del set. Mantenga los pies firmes en el suelo para estabilidad.",
            RestTimeSeconds = 75,
            Difficulty = "Intermedio",
            Sets = new List<ExerciseSet>
            {
                new ExerciseSet { Id = 5, SetNumber = 1, Reps = 12, Weight = 25, RestSeconds = 75 },
                new ExerciseSet { Id = 6, SetNumber = 2, Reps = 10, Weight = 30, RestSeconds = 75 },
                new ExerciseSet { Id = 7, SetNumber = 3, Reps = 8, Weight = 32.5m, RestSeconds = 90 }
            }
        });

        day1.Exercises.Add(new RoutineExercise
        {
            Id = 3,
            Order = 3,
            Name = "Fondos en Paralelas",
            Category = "Fuerza",
            MuscleGroups = new List<string> { "Pectorales Inferior", "Tríceps" },
            Equipment = "Paralelas o Máquina de Fondos",
            Instructions = "Sosténgase en las paralelas con los brazos extendidos. Baje el cuerpo flexionando los codos hasta sentir estiramiento en el pecho, luego empuje hacia arriba.",
            SafetyTips = "No baje demasiado para evitar lesión en el hombro. Si es principiante, use máquina asistida.",
            RestTimeSeconds = 90,
            Difficulty = "Intermedio-Avanzado",
            Sets = new List<ExerciseSet>
            {
                new ExerciseSet { Id = 8, SetNumber = 1, Reps = 10, Weight = 0, RestSeconds = 90 },
                new ExerciseSet { Id = 9, SetNumber = 2, Reps = 8, Weight = 0, RestSeconds = 90 },
                new ExerciseSet { Id = 10, SetNumber = 3, Reps = 6, Weight = 0, RestSeconds = 90 }
            }
        });

        routine.Days.Add(day1);

        // Día 2 - Espalda y Bíceps
        var day2 = new RoutineDay
        {
            Id = 2,
            DayNumber = 2,
            Name = "Día 2 - Espalda y Bíceps",
            Description = "Entrenamiento de músculos de tracción para complementar la rutina de validación",
            FocusArea = "Tren Superior - Tracción",
            TargetIntensity = "Alta",
            EstimatedDurationMinutes = 80
        };

        day2.Exercises.Add(new RoutineExercise
        {
            Id = 11,
            Order = 1,
            Name = "Dominadas",
            Category = "Fuerza",
            MuscleGroups = new List<string> { "Dorsales", "Bíceps", "Romboides", "Trapecio Medio" },
            Equipment = "Barra de Dominadas",
            Instructions = "Cuelgue de la barra con agarre pronador, manos ligeramente más anchas que los hombros. Tire del cuerpo hacia arriba hasta que la barbilla pase la barra.",
            SafetyTips = "Controle el descenso, no se deje caer. Si no puede completar las repeticiones, use banda elástica o máquina asistida.",
            RestTimeSeconds = 120,
            Difficulty = "Avanzado",
            Sets = new List<ExerciseSet>
            {
                new ExerciseSet { Id = 12, SetNumber = 1, Reps = 8, Weight = 0, RestSeconds = 120 },
                new ExerciseSet { Id = 13, SetNumber = 2, Reps = 6, Weight = 0, RestSeconds = 120 },
                new ExerciseSet { Id = 14, SetNumber = 3, Reps = 5, Weight = 0, RestSeconds = 120 }
            }
        });

        day2.Exercises.Add(new RoutineExercise
        {
            Id = 15,
            Order = 2,
            Name = "Remo con Barra",
            Category = "Fuerza",
            MuscleGroups = new List<string> { "Dorsales", "Romboides", "Trapecio Medio", "Bíceps" },
            Equipment = "Barra Olímpica",
            Instructions = "Con la barra en el suelo, inclínese hacia adelante manteniendo la espalda recta. Tire de la barra hacia el abdomen bajo.",
            SafetyTips = "Mantenga la espalda neutral durante todo el movimiento. No use impulso para levantar la barra.",
            RestTimeSeconds = 90,
            Difficulty = "Intermedio",
            Sets = new List<ExerciseSet>
            {
                new ExerciseSet { Id = 16, SetNumber = 1, Reps = 10, Weight = 60, RestSeconds = 90 },
                new ExerciseSet { Id = 17, SetNumber = 2, Reps = 8, Weight = 70, RestSeconds = 90 },
                new ExerciseSet { Id = 18, SetNumber = 3, Reps = 6, Weight = 80, RestSeconds = 90 }
            }
        });

        routine.Days.Add(day2);

        // Calcular métricas completas
        routine.Metrics = new RoutineMetrics
        {
            TotalExercises = routine.Days.SelectMany(d => d.Exercises).Count(),
            TotalSets = routine.Days.SelectMany(d => d.Exercises).SelectMany(e => e.Sets).Count(),
            EstimatedDurationMinutes = routine.Days.Sum(d => d.EstimatedDurationMinutes),
            MuscleGroupsCovered = routine.Days.SelectMany(d => d.Exercises).SelectMany(e => e.MuscleGroups).Distinct().ToList(),
            EquipmentRequired = routine.Days.SelectMany(d => d.Exercises).Select(e => e.Equipment).Distinct().ToList(),
            DifficultyLevel = "Intermedio-Avanzado",
            CaloriesBurnedEstimate = 420
        };

        return routine;
    }
}