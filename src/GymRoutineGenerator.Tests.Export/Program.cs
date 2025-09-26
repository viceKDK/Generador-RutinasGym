using System;
using System.Threading.Tasks;
using GymRoutineGenerator.Core.Models.Routines;
using GymRoutineGenerator.Core.Services.Documents;
using GymRoutineGenerator.Infrastructure.Documents;
using System.Collections.Generic;
using System.Linq;

namespace GymRoutineGenerator.Tests.Export;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Test de Exportación a Word - Epic 5 Story 5.5 ===");
        Console.WriteLine();

        try
        {
            // Crear servicios
            var wordDocumentService = new WordDocumentService();
            var templateManagerService = new TemplateManagerService();
            var exportService = new SimpleExportService(wordDocumentService, templateManagerService);

            Console.WriteLine("✓ Servicios inicializados correctamente");

            // Crear rutina de prueba
            var routine = CreateTestRoutine();
            Console.WriteLine($"✓ Rutina de prueba creada: {routine.Name}");
            Console.WriteLine($"  Cliente: {routine.ClientName}");
            Console.WriteLine($"  Días: {routine.Days.Count}");
            Console.WriteLine($"  Ejercicios totales: {routine.Metrics.TotalExercises}");
            Console.WriteLine();

            // Probar plantillas disponibles
            Console.WriteLine("📋 Probando plantillas disponibles:");
            var templates = await templateManagerService.GetAvailableTemplatesAsync();
            foreach (var template in templates)
            {
                Console.WriteLine($"  - {template.Name} ({template.Type})");
            }
            Console.WriteLine();

            // Probar exportación con diferentes plantillas
            var templateIds = new[] { "standard", "professional", "gym" };

            foreach (var templateId in templateIds)
            {
                Console.WriteLine($"🔄 Probando exportación con plantilla: {templateId}");

                var options = new ExportOptions
                {
                    AutoOpenAfterExport = false,
                    OverwriteExisting = true,
                    CreateBackup = false
                };

                var progress = new Progress<ExportProgress>(p =>
                {
                    Console.Write($"\r  Progreso: {p.PercentComplete:F1}% - {p.CurrentOperation}");
                });

                var result = await exportService.ExportRoutineToWordAsync(routine, templateId, options, progress);
                Console.WriteLine(); // Nueva línea después del progreso

                if (result.Success)
                {
                    Console.WriteLine($"  ✓ Exportación exitosa");
                    Console.WriteLine($"    Archivo: {result.FilePath}");
                    Console.WriteLine($"    Tamaño: {result.FileSizeBytes / 1024:N0} KB");
                    Console.WriteLine($"    Tiempo: {result.ExportDuration.TotalSeconds:F2} segundos");
                    Console.WriteLine($"    Ejercicios: {result.ExerciseCount}");
                }
                else
                {
                    Console.WriteLine($"  ✗ Error en exportación: {result.ErrorMessage}");
                }
                Console.WriteLine();
            }

            // Probar exportación múltiple
            Console.WriteLine("🔄 Probando exportación múltiple...");
            var routines = new List<Routine>
            {
                CreateTestRoutine("Cliente A", "Rutina Fuerza A"),
                CreateTestRoutine("Cliente B", "Rutina Cardio B"),
                CreateTestRoutine("Cliente C", "Rutina Híbrida C")
            };

            var multiOptions = new ExportOptions
            {
                AutoOpenAfterExport = false,
                OverwriteExisting = true
            };

            var multiProgress = new Progress<ExportProgress>(p =>
            {
                Console.Write($"\r  Progreso múltiple: {p.PercentComplete:F1}% - {p.CurrentOperation}");
            });

            var multiResult = await exportService.ExportMultipleRoutinesToWordAsync(
                routines, "professional", multiOptions, multiProgress);

            Console.WriteLine(); // Nueva línea después del progreso

            if (multiResult.Success)
            {
                Console.WriteLine($"  ✓ Exportación múltiple exitosa");
                Console.WriteLine($"    Directorio: {multiResult.FilePath}");
                Console.WriteLine($"    Tamaño total: {multiResult.FileSizeBytes / 1024:N0} KB");
                Console.WriteLine($"    Tiempo total: {multiResult.ExportDuration.TotalSeconds:F2} segundos");
                Console.WriteLine($"    Ejercicios totales: {multiResult.ExerciseCount}");
            }
            else
            {
                Console.WriteLine($"  ✗ Error en exportación múltiple: {multiResult.ErrorMessage}");
            }

            Console.WriteLine();
            Console.WriteLine("=== Pruebas completadas ===");
            Console.WriteLine();
            Console.WriteLine("Presiona cualquier tecla para continuar...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error durante las pruebas: {ex.Message}");
            Console.WriteLine($"   {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            }
            Console.WriteLine();
            Console.WriteLine("Presiona cualquier tecla para salir...");
            Console.ReadKey();
        }
    }

    static Routine CreateTestRoutine(string clientName = "Juan Pérez", string routineName = "Rutina de Fuerza - Semana 1")
    {
        var routine = new Routine
        {
            Id = 1,
            Name = routineName,
            ClientName = clientName,
            Description = "Rutina de entrenamiento de fuerza diseñada para desarrollo muscular progresivo.",
            Goal = "Desarrollo de fuerza y masa muscular",
            DurationWeeks = 4,
            CreatedDate = DateTime.Now
        };

        // Día 1 - Pecho y Tríceps
        var day1 = new RoutineDay
        {
            Id = 1,
            DayNumber = 1,
            Name = "Día 1 - Pecho y Tríceps",
            Description = "Enfoque en músculos del pecho y tríceps",
            FocusArea = "Tren Superior",
            TargetIntensity = "Alta",
            EstimatedDurationMinutes = 75
        };

        day1.Exercises.AddRange(new[]
        {
            new RoutineExercise
            {
                Id = 1,
                Order = 1,
                Name = "Press de Banca",
                Category = "Fuerza",
                MuscleGroups = new List<string> { "Pectorales", "Tríceps", "Deltoides Anterior" },
                Equipment = "Barra y Banco",
                Instructions = "Acuéstate en el banco con los pies firmes en el suelo. Toma la barra con un agarre ligeramente más ancho que los hombros.",
                SafetyTips = "Mantén los omóplatos retraídos. No rebotes la barra en el pecho.",
                Sets = new List<ExerciseSet>
                {
                    new ExerciseSet { Id = 1, SetNumber = 1, Reps = 12, Weight = 60, RestSeconds = 90 },
                    new ExerciseSet { Id = 2, SetNumber = 2, Reps = 10, Weight = 70, RestSeconds = 90 },
                    new ExerciseSet { Id = 3, SetNumber = 3, Reps = 8, Weight = 80, RestSeconds = 120 },
                    new ExerciseSet { Id = 4, SetNumber = 4, Reps = 6, Weight = 85, RestSeconds = 120 }
                },
                RestTimeSeconds = 90,
                Difficulty = "Intermedio"
            },
            new RoutineExercise
            {
                Id = 2,
                Order = 2,
                Name = "Press Inclinado con Mancuernas",
                Category = "Fuerza",
                MuscleGroups = new List<string> { "Pectorales Superior", "Deltoides Anterior" },
                Equipment = "Mancuernas y Banco Inclinado",
                Instructions = "Ajusta el banco a 30-45°. Toma las mancuernas con agarre neutro al inicio.",
                SafetyTips = "Controla el descenso. No dejes caer las mancuernas.",
                Sets = new List<ExerciseSet>
                {
                    new ExerciseSet { Id = 5, SetNumber = 1, Reps = 12, Weight = 25, RestSeconds = 75 },
                    new ExerciseSet { Id = 6, SetNumber = 2, Reps = 10, Weight = 30, RestSeconds = 75 },
                    new ExerciseSet { Id = 7, SetNumber = 3, Reps = 8, Weight = 32.5m, RestSeconds = 90 }
                },
                RestTimeSeconds = 75,
                Difficulty = "Intermedio"
            },
            new RoutineExercise
            {
                Id = 3,
                Order = 3,
                Name = "Fondos en Paralelas",
                Category = "Peso Corporal",
                MuscleGroups = new List<string> { "Tríceps", "Pectorales Inferior", "Deltoides Anterior" },
                Equipment = "Paralelas",
                Instructions = "Sujétate a las paralelas con los brazos extendidos. Baja controladamente hasta sentir estiramiento en el pecho.",
                SafetyTips = "No bajes más de lo que tu flexibilidad permita. Mantén el core activo.",
                Sets = new List<ExerciseSet>
                {
                    new ExerciseSet { Id = 8, SetNumber = 1, Reps = 10, Weight = 0, RestSeconds = 60 },
                    new ExerciseSet { Id = 9, SetNumber = 2, Reps = 8, Weight = 0, RestSeconds = 60 },
                    new ExerciseSet { Id = 10, SetNumber = 3, Reps = 6, Weight = 0, RestSeconds = 90 }
                },
                RestTimeSeconds = 60,
                Difficulty = "Intermedio"
            }
        });

        routine.Days.Add(day1);

        // Día 2 - Espalda y Bíceps
        var day2 = new RoutineDay
        {
            Id = 2,
            DayNumber = 2,
            Name = "Día 2 - Espalda y Bíceps",
            Description = "Enfoque en músculos de la espalda y bíceps",
            FocusArea = "Tren Superior",
            TargetIntensity = "Alta",
            EstimatedDurationMinutes = 80
        };

        day2.Exercises.AddRange(new[]
        {
            new RoutineExercise
            {
                Id = 4,
                Order = 1,
                Name = "Dominadas",
                Category = "Peso Corporal",
                MuscleGroups = new List<string> { "Dorsales", "Bíceps", "Romboides" },
                Equipment = "Barra de Dominadas",
                Instructions = "Cuelga de la barra con agarre pronador. Tira del cuerpo hacia arriba hasta que la barbilla pase la barra.",
                SafetyTips = "Controla el descenso. Si no puedes hacer dominadas completas, usa banda elástica.",
                Sets = new List<ExerciseSet>
                {
                    new ExerciseSet { Id = 11, SetNumber = 1, Reps = 8, Weight = 0, RestSeconds = 90 },
                    new ExerciseSet { Id = 12, SetNumber = 2, Reps = 6, Weight = 0, RestSeconds = 90 },
                    new ExerciseSet { Id = 13, SetNumber = 3, Reps = 5, Weight = 0, RestSeconds = 120 }
                },
                RestTimeSeconds = 90,
                Difficulty = "Avanzado"
            },
            new RoutineExercise
            {
                Id = 5,
                Order = 2,
                Name = "Remo con Barra",
                Category = "Fuerza",
                MuscleGroups = new List<string> { "Dorsales", "Romboides", "Trapecio Medio", "Bíceps" },
                Equipment = "Barra",
                Instructions = "De pie con la barra, inclínate hacia adelante manteniendo la espalda recta. Tira de la barra hacia el abdomen.",
                SafetyTips = "Mantén la espalda neutral. No uses impulso para levantar el peso.",
                Sets = new List<ExerciseSet>
                {
                    new ExerciseSet { Id = 14, SetNumber = 1, Reps = 12, Weight = 50, RestSeconds = 75 },
                    new ExerciseSet { Id = 15, SetNumber = 2, Reps = 10, Weight = 55, RestSeconds = 75 },
                    new ExerciseSet { Id = 16, SetNumber = 3, Reps = 8, Weight = 60, RestSeconds = 90 }
                },
                RestTimeSeconds = 75,
                Difficulty = "Intermedio"
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
            DifficultyLevel = "Intermedio",
            CaloriesBurnedEstimate = 350
        };

        return routine;
    }
}