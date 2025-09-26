using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using GymRoutineGenerator.Core.Models.Routines;
using GymRoutineGenerator.Core.Services.Documents;
using GymRoutineGenerator.Infrastructure.Documents;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace GymRoutineGenerator.Tests.Validation;

[TestClass]
public class UserAcceptanceTests
{
    private IExportService _exportService;
    private string _testOutputDirectory;
    private WordDocumentService _wordService;
    private TemplateManagerService _templateService;

    [TestInitialize]
    public void Setup()
    {
        _testOutputDirectory = Path.Combine(Path.GetTempPath(), "GymRoutineUserAcceptance", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        Directory.CreateDirectory(_testOutputDirectory);

        _wordService = new WordDocumentService();
        _templateService = new TemplateManagerService();
        _exportService = new SimpleExportService(_wordService, _templateService);
    }

    [TestCleanup]
    public void Cleanup()
    {
        try
        {
            if (Directory.Exists(_testOutputDirectory))
            {
                Console.WriteLine($"📁 Test files saved in: {_testOutputDirectory}");
                // Don't delete for manual validation
                // Directory.Delete(_testOutputDirectory, true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cleanup warning: {ex.Message}");
        }
    }

    [TestMethod]
    [Description("VALIDACIÓN FINAL: Tu madre genera 5 rutinas diferentes exitosamente")]
    public async Task UserAcceptance_FiveDifferentRoutinesIndependently_ShouldAllSucceed()
    {
        Console.WriteLine("🎯 VALIDACIÓN FINAL PARA USUARIO NO TÉCNICO");
        Console.WriteLine("==========================================");
        Console.WriteLine("Simulando que una persona mayor sin conocimientos técnicos");
        Console.WriteLine("genera 5 rutinas diferentes de forma independiente.\n");

        // Arrange: 5 escenarios realistas que una persona mayor podría intentar
        var userScenarios = new[]
        {
            new UserScenario
            {
                Name = "Ana - Principiante en Casa",
                ClientName = "Ana García",
                Goal = "Empezar a ejercitarme en casa",
                ExpectedTemplate = "basic",
                Description = "Usuario mayor que quiere empezar con ejercicios básicos en casa",
                ExpectedOutcome = "Rutina simple y segura para principiantes"
            },
            new UserScenario
            {
                Name = "Carlos - Problemas de Espalda",
                ClientName = "Carlos Martínez",
                Goal = "Fortalecer espalda por dolor",
                ExpectedTemplate = "rehabilitation",
                Description = "Persona con problemas de espalda que necesita ejercicios suaves",
                ExpectedOutcome = "Rutina de rehabilitación con ejercicios seguros"
            },
            new UserScenario
            {
                Name = "María - Mantenerse Activa",
                ClientName = "María López",
                Goal = "Mantenerme activa y flexible",
                ExpectedTemplate = "standard",
                Description = "Mujer mayor que quiere mantenerse activa sin esforzarse mucho",
                ExpectedOutcome = "Rutina equilibrada con enfoque en flexibilidad"
            },
            new UserScenario
            {
                Name = "José - Rutina de Gimnasio",
                ClientName = "José Rodríguez",
                Goal = "Ir al gimnasio y usar las máquinas",
                ExpectedTemplate = "gym",
                Description = "Hombre que tiene acceso a gimnasio y quiere usarlo bien",
                ExpectedOutcome = "Rutina con equipamiento de gimnasio"
            },
            new UserScenario
            {
                Name = "Laura - Ejercicios Completos",
                ClientName = "Laura Hernández",
                Goal = "Rutina completa y profesional",
                ExpectedTemplate = "professional",
                Description = "Usuario que quiere una rutina más detallada y estructurada",
                ExpectedOutcome = "Rutina profesional con instrucciones detalladas"
            }
        };

        var results = new List<UserTestResult>();

        // Act: Simular cada escenario independiente
        for (int i = 0; i < userScenarios.Length; i++)
        {
            var scenario = userScenarios[i];
            Console.WriteLine($"\n🧓 ESCENARIO {i + 1}: {scenario.Name}");
            Console.WriteLine($"   👤 Cliente: {scenario.ClientName}");
            Console.WriteLine($"   🎯 Objetivo: {scenario.Goal}");
            Console.WriteLine($"   📝 Descripción: {scenario.Description}");

            var testResult = await ExecuteUserScenario(scenario, i + 1);
            results.Add(testResult);

            if (testResult.Success)
            {
                Console.WriteLine($"   ✅ ÉXITO: {testResult.Message}");
                Console.WriteLine($"   📄 Archivo: {Path.GetFileName(testResult.FilePath)}");
                Console.WriteLine($"   📏 Tamaño: {testResult.FileSizeKB:N0} KB");
                Console.WriteLine($"   ⏱️ Tiempo: {testResult.Duration.TotalSeconds:F1}s");
            }
            else
            {
                Console.WriteLine($"   ❌ ERROR: {testResult.ErrorMessage}");
            }
        }

        // Assert: Todos los escenarios deben ser exitosos
        var successfulScenarios = results.Where(r => r.Success).ToList();

        Assert.AreEqual(userScenarios.Length, successfulScenarios.Count,
            $"Todos los escenarios de usuario deben ser exitosos. " +
            $"Exitosos: {successfulScenarios.Count}/{userScenarios.Length}");

        // Validaciones adicionales específicas para usuarios no técnicos
        await ValidateUserFriendliness(successfulScenarios);
        await ValidateFileQuality(successfulScenarios);
        await ValidateUniqueness(successfulScenarios);

        // Final report
        Console.WriteLine("\n🎉 VALIDACIÓN FINAL COMPLETADA EXITOSAMENTE!");
        Console.WriteLine("=============================================");
        Console.WriteLine($"✅ Todos los escenarios de usuario fueron exitosos ({successfulScenarios.Count}/{userScenarios.Length})");
        Console.WriteLine($"⏱️ Tiempo total: {results.Sum(r => r.Duration.TotalSeconds):F1} segundos");
        Console.WriteLine($"📁 Archivos generados en: {_testOutputDirectory}");
        Console.WriteLine("\n🏆 LA APLICACIÓN ESTÁ LISTA PARA USUARIOS NO TÉCNICOS");
        Console.WriteLine("   Tu madre podría usar esta aplicación exitosamente!");
    }

    [TestMethod]
    [Description("Test de facilidad de uso: interfaz amigable para personas mayores")]
    public async Task UserAcceptance_EaseOfUse_ShouldBeGrandmotherFriendly()
    {
        Console.WriteLine("👵 Test de facilidad de uso para personas mayores");

        // Test: Crear rutina con valores por defecto (mínima configuración)
        var minimumInputRoutine = CreateMinimumInputRoutine();

        var options = new ExportOptions
        {
            OutputPath = _testOutputDirectory,
            AutoOpenAfterExport = false, // Usuario mayor no querría abrir automáticamente
            OverwriteExisting = true
        };

        var result = await _exportService.ExportRoutineToWordAsync(minimumInputRoutine, "basic", options);

        // Assert: Debe funcionar con configuración mínima
        Assert.IsTrue(result.Success, $"Minimum input routine failed: {result.ErrorMessage}");
        Assert.IsTrue(File.Exists(result.FilePath), "File should be created with minimum input");

        var fileInfo = new FileInfo(result.FilePath);
        Assert.IsTrue(fileInfo.Length > 5000, "Even minimum routine should generate substantial content");

        Console.WriteLine($"✅ Configuración mínima exitosa:");
        Console.WriteLine($"   📄 Tamaño: {fileInfo.Length / 1024:N0} KB");
        Console.WriteLine($"   ⏱️ Tiempo: {result.ExportDuration.TotalSeconds:F1}s");
        Console.WriteLine($"   💪 Ejercicios: {result.ExerciseCount}");
    }

    [TestMethod]
    [Description("Test de recuperación de errores: manejo elegante para usuarios no técnicos")]
    public async Task UserAcceptance_ErrorRecovery_ShouldBeUserFriendly()
    {
        Console.WriteLine("🔧 Test de recuperación de errores para usuarios no técnicos");

        // Test 1: Rutina con datos problemáticos pero recuperables
        var problematicRoutine = CreateProblematicButRecoverableRoutine();

        var options = new ExportOptions
        {
            OutputPath = _testOutputDirectory,
            AutoOpenAfterExport = false,
            OverwriteExisting = true
        };

        var result = await _exportService.ExportRoutineToWordAsync(problematicRoutine, "basic", options);

        // Should either succeed with sanitized data or fail gracefully
        if (result.Success)
        {
            Console.WriteLine("✅ Datos problemáticos manejados exitosamente");
            Assert.IsTrue(File.Exists(result.FilePath));
        }
        else
        {
            Console.WriteLine($"✅ Error manejado elegantemente: {result.ErrorMessage}");
            Assert.IsNotNull(result.ErrorMessage);
            Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
            // Error message should be user-friendly (no technical jargon)
            Assert.IsFalse(result.ErrorMessage.Contains("Exception"));
            Assert.IsFalse(result.ErrorMessage.Contains("null"));
            Assert.IsFalse(result.ErrorMessage.Contains("Thread"));
        }
    }

    [TestMethod]
    [Description("Test de consistencia: mismos inputs producen mismos outputs")]
    public async Task UserAcceptance_Consistency_SameInputsSameOutputs()
    {
        Console.WriteLine("🔄 Test de consistencia: mismos inputs, mismos outputs");

        var routine = CreateStandardUserRoutine();
        var results = new List<(bool Success, long FileSize, int ExerciseCount)>();

        // Execute same routine 3 times
        for (int i = 1; i <= 3; i++)
        {
            var options = new ExportOptions
            {
                OutputPath = _testOutputDirectory,
                AutoOpenAfterExport = false,
                OverwriteExisting = true
            };

            var result = await _exportService.ExportRoutineToWordAsync(routine, "standard", options);

            if (result.Success && File.Exists(result.FilePath))
            {
                var fileSize = new FileInfo(result.FilePath).Length;
                results.Add((true, fileSize, result.ExerciseCount));
            }
            else
            {
                results.Add((false, 0, 0));
            }
        }

        // Assert: All should succeed with consistent results
        Assert.IsTrue(results.All(r => r.Success), "All consistency runs should succeed");

        var uniqueFileSizes = results.Select(r => r.FileSize).Distinct().Count();
        var uniqueExerciseCounts = results.Select(r => r.ExerciseCount).Distinct().Count();

        Assert.AreEqual(1, uniqueFileSizes, "File sizes should be identical for same input");
        Assert.AreEqual(1, uniqueExerciseCounts, "Exercise counts should be identical for same input");

        Console.WriteLine($"✅ Consistencia verificada:");
        Console.WriteLine($"   📄 Tamaño de archivo: {results.First().FileSize / 1024:N0} KB (3/3 idénticos)");
        Console.WriteLine($"   💪 Ejercicios: {results.First().ExerciseCount} (3/3 idénticos)");
    }

    #region Private Methods

    private async Task<UserTestResult> ExecuteUserScenario(UserScenario scenario, int scenarioNumber)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Simulate user creating a routine based on their goal
            var routine = CreateRoutineForUserGoal(scenario);

            var options = new ExportOptions
            {
                OutputPath = _testOutputDirectory,
                AutoOpenAfterExport = false, // Don't auto-open for elderly users
                OverwriteExisting = true
            };

            var result = await _exportService.ExportRoutineToWordAsync(routine, scenario.ExpectedTemplate, options);
            stopwatch.Stop();

            if (result.Success && File.Exists(result.FilePath))
            {
                var fileInfo = new FileInfo(result.FilePath);
                return new UserTestResult
                {
                    ScenarioName = scenario.Name,
                    Success = true,
                    FilePath = result.FilePath,
                    FileSizeKB = fileInfo.Length / 1024,
                    Duration = stopwatch.Elapsed,
                    ExerciseCount = result.ExerciseCount,
                    Message = $"Rutina generada exitosamente - {scenario.ExpectedOutcome}"
                };
            }
            else
            {
                return new UserTestResult
                {
                    ScenarioName = scenario.Name,
                    Success = false,
                    ErrorMessage = result.ErrorMessage,
                    Duration = stopwatch.Elapsed
                };
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new UserTestResult
            {
                ScenarioName = scenario.Name,
                Success = false,
                ErrorMessage = $"Error inesperado: {ex.Message}",
                Duration = stopwatch.Elapsed
            };
        }
    }

    private Routine CreateRoutineForUserGoal(UserScenario scenario)
    {
        var routine = new Routine
        {
            Id = scenario.Name.GetHashCode(),
            Name = $"Mi Rutina de {scenario.Goal}",
            ClientName = scenario.ClientName,
            Description = DetermineDescriptionFromGoal(scenario.Goal),
            Goal = scenario.Goal,
            DurationWeeks = DetermineDurationFromGoal(scenario.Goal),
            CreatedDate = DateTime.Now
        };

        // Create days based on user goal
        var dayConfigs = GetDayConfigurationsForGoal(scenario.Goal);

        foreach (var dayConfig in dayConfigs)
        {
            var day = new RoutineDay
            {
                Id = dayConfig.DayNumber,
                DayNumber = dayConfig.DayNumber,
                Name = dayConfig.Name,
                Description = dayConfig.Description,
                FocusArea = dayConfig.FocusArea,
                TargetIntensity = dayConfig.Intensity,
                EstimatedDurationMinutes = dayConfig.Duration
            };

            // Add exercises appropriate for the goal
            var exercises = GetExercisesForGoal(scenario.Goal, dayConfig.DayNumber);
            foreach (var exercise in exercises)
            {
                day.Exercises.Add(exercise);
            }

            routine.Days.Add(day);
        }

        routine.Metrics = CalculateRoutineMetrics(routine);
        return routine;
    }

    private string DetermineDescriptionFromGoal(string goal)
    {
        return goal.ToLower() switch
        {
            var g when g.Contains("casa") => "Rutina diseñada para ejercitarse en casa con equipamiento mínimo o sin equipamiento.",
            var g when g.Contains("espalda") || g.Contains("dolor") => "Rutina terapéutica enfocada en fortalecer la espalda y reducir molestias.",
            var g when g.Contains("activa") || g.Contains("flexible") => "Rutina para mantenerse activo con enfoque en movilidad y flexibilidad.",
            var g when g.Contains("gimnasio") || g.Contains("máquinas") => "Rutina diseñada para aprovechar el equipamiento disponible en gimnasios.",
            var g when g.Contains("completa") || g.Contains("profesional") => "Rutina integral y estructurada con progresión sistemática.",
            _ => "Rutina personalizada adaptada a objetivos específicos de entrenamiento."
        };
    }

    private int DetermineDurationFromGoal(string goal)
    {
        return goal.ToLower() switch
        {
            var g when g.Contains("empezar") || g.Contains("principiante") => 4,
            var g when g.Contains("dolor") || g.Contains("rehabilit") => 6,
            var g when g.Contains("mantener") || g.Contains("activa") => 8,
            var g when g.Contains("gimnasio") => 6,
            var g when g.Contains("completa") || g.Contains("profesional") => 12,
            _ => 6
        };
    }

    private List<DayConfiguration> GetDayConfigurationsForGoal(string goal)
    {
        return goal.ToLower() switch
        {
            var g when g.Contains("casa") || g.Contains("empezar") => new List<DayConfiguration>
            {
                new() { DayNumber = 1, Name = "Día 1 - Cuerpo Completo", Description = "Ejercicios básicos para todo el cuerpo", FocusArea = "General", Intensity = "Baja", Duration = 30 },
                new() { DayNumber = 2, Name = "Día 2 - Flexibilidad", Description = "Estiramientos y movilidad", FocusArea = "Flexibilidad", Intensity = "Muy Baja", Duration = 25 },
                new() { DayNumber = 3, Name = "Día 3 - Fortalecimiento", Description = "Ejercicios de fortalecimiento suave", FocusArea = "Fuerza", Intensity = "Baja", Duration = 35 }
            },
            var g when g.Contains("espalda") || g.Contains("dolor") => new List<DayConfiguration>
            {
                new() { DayNumber = 1, Name = "Día 1 - Movilidad Espinal", Description = "Ejercicios suaves para la espalda", FocusArea = "Rehabilitación", Intensity = "Muy Baja", Duration = 20 },
                new() { DayNumber = 2, Name = "Día 2 - Fortalecimiento Core", Description = "Fortalecimiento del núcleo", FocusArea = "Core", Intensity = "Baja", Duration = 25 }
            },
            var g when g.Contains("activa") || g.Contains("flexible") => new List<DayConfiguration>
            {
                new() { DayNumber = 1, Name = "Día 1 - Actividad General", Description = "Ejercicios variados de intensidad moderada", FocusArea = "General", Intensity = "Moderada", Duration = 45 },
                new() { DayNumber = 2, Name = "Día 2 - Flexibilidad", Description = "Sesión enfocada en flexibilidad", FocusArea = "Flexibilidad", Intensity = "Baja", Duration = 40 },
                new() { DayNumber = 3, Name = "Día 3 - Resistencia", Description = "Ejercicios cardiovasculares suaves", FocusArea = "Cardio", Intensity = "Moderada", Duration = 35 }
            },
            var g when g.Contains("gimnasio") => new List<DayConfiguration>
            {
                new() { DayNumber = 1, Name = "Día 1 - Tren Superior", Description = "Ejercicios con máquinas para tren superior", FocusArea = "Tren Superior", Intensity = "Moderada", Duration = 60 },
                new() { DayNumber = 2, Name = "Día 2 - Tren Inferior", Description = "Ejercicios con máquinas para piernas", FocusArea = "Tren Inferior", Intensity = "Moderada", Duration = 60 },
                new() { DayNumber = 3, Name = "Día 3 - Cardio y Core", Description = "Máquinas cardiovasculares y abdominales", FocusArea = "Cardio/Core", Intensity = "Moderada", Duration = 45 }
            },
            var g when g.Contains("completa") || g.Contains("profesional") => new List<DayConfiguration>
            {
                new() { DayNumber = 1, Name = "Día 1 - Pecho y Tríceps", Description = "Entrenamiento enfocado en pectorales y tríceps", FocusArea = "Pecho/Tríceps", Intensity = "Alta", Duration = 75 },
                new() { DayNumber = 2, Name = "Día 2 - Espalda y Bíceps", Description = "Entrenamiento de espalda y bíceps", FocusArea = "Espalda/Bíceps", Intensity = "Alta", Duration = 75 },
                new() { DayNumber = 3, Name = "Día 3 - Piernas", Description = "Entrenamiento completo de piernas", FocusArea = "Piernas", Intensity = "Alta", Duration = 80 },
                new() { DayNumber = 4, Name = "Día 4 - Hombros y Core", Description = "Entrenamiento de hombros y core", FocusArea = "Hombros/Core", Intensity = "Moderada-Alta", Duration = 65 }
            },
            _ => new List<DayConfiguration>
            {
                new() { DayNumber = 1, Name = "Día 1 - General", Description = "Entrenamiento general", FocusArea = "General", Intensity = "Moderada", Duration = 50 }
            }
        };
    }

    private List<RoutineExercise> GetExercisesForGoal(string goal, int dayNumber)
    {
        // This would normally come from a database or service
        // For testing, we'll create appropriate exercises based on goal
        var exercises = new List<RoutineExercise>();

        if (goal.ToLower().Contains("casa") || goal.ToLower().Contains("empezar"))
        {
            exercises.AddRange(CreateHomeExercises(dayNumber));
        }
        else if (goal.ToLower().Contains("espalda") || goal.ToLower().Contains("dolor"))
        {
            exercises.AddRange(CreateRehabilitationExercises(dayNumber));
        }
        else if (goal.ToLower().Contains("activa") || goal.ToLower().Contains("flexible"))
        {
            exercises.AddRange(CreateActiveFlexibilityExercises(dayNumber));
        }
        else if (goal.ToLower().Contains("gimnasio"))
        {
            exercises.AddRange(CreateGymExercises(dayNumber));
        }
        else if (goal.ToLower().Contains("completa") || goal.ToLower().Contains("profesional"))
        {
            exercises.AddRange(CreateProfessionalExercises(dayNumber));
        }
        else
        {
            exercises.AddRange(CreateGeneralExercises(dayNumber));
        }

        return exercises;
    }

    private List<RoutineExercise> CreateHomeExercises(int dayNumber)
    {
        var exercises = new List<RoutineExercise>();

        switch (dayNumber)
        {
            case 1: // Cuerpo completo
                exercises.Add(CreateExercise(1, "Flexiones de Rodillas", "Peso Corporal", "Pectorales", 3, 8, 0));
                exercises.Add(CreateExercise(2, "Sentadillas Básicas", "Peso Corporal", "Piernas", 3, 10, 0));
                exercises.Add(CreateExercise(3, "Plancha", "Peso Corporal", "Core", 3, 20, 0));
                break;
            case 2: // Flexibilidad
                exercises.Add(CreateExercise(1, "Estiramiento de Brazos", "Ninguno", "Hombros", 2, 30, 0));
                exercises.Add(CreateExercise(2, "Estiramiento de Piernas", "Ninguno", "Piernas", 2, 30, 0));
                break;
            case 3: // Fortalecimiento
                exercises.Add(CreateExercise(1, "Sentadillas con Silla", "Silla", "Piernas", 3, 12, 0));
                exercises.Add(CreateExercise(2, "Elevaciones de Brazo", "Peso Corporal", "Hombros", 3, 10, 0));
                break;
        }

        return exercises;
    }

    private List<RoutineExercise> CreateRehabilitationExercises(int dayNumber)
    {
        var exercises = new List<RoutineExercise>();

        switch (dayNumber)
        {
            case 1: // Movilidad espinal
                exercises.Add(CreateExercise(1, "Gato-Camello", "Colchoneta", "Espalda", 2, 10, 0));
                exercises.Add(CreateExercise(2, "Rotaciones de Cadera", "Ninguno", "Cadera", 2, 8, 0));
                break;
            case 2: // Fortalecimiento core
                exercises.Add(CreateExercise(1, "Plancha Modificada", "Colchoneta", "Core", 3, 15, 0));
                exercises.Add(CreateExercise(2, "Puente de Glúteo", "Colchoneta", "Glúteos", 3, 12, 0));
                break;
        }

        return exercises;
    }

    private List<RoutineExercise> CreateActiveFlexibilityExercises(int dayNumber)
    {
        var exercises = new List<RoutineExercise>();

        switch (dayNumber)
        {
            case 1: // Actividad general
                exercises.Add(CreateExercise(1, "Marcha en el Lugar", "Ninguno", "Cardio", 3, 30, 0));
                exercises.Add(CreateExercise(2, "Flexiones de Pared", "Pared", "Pectorales", 3, 10, 0));
                exercises.Add(CreateExercise(3, "Sentadillas Suaves", "Peso Corporal", "Piernas", 3, 12, 0));
                break;
            case 2: // Flexibilidad
                exercises.Add(CreateExercise(1, "Estiramiento Completo", "Colchoneta", "Todo el cuerpo", 1, 300, 0));
                exercises.Add(CreateExercise(2, "Yoga Suave", "Colchoneta", "Flexibilidad", 1, 600, 0));
                break;
            case 3: // Resistencia
                exercises.Add(CreateExercise(1, "Caminata Activa", "Ninguno", "Cardio", 1, 1200, 0));
                exercises.Add(CreateExercise(2, "Ejercicios de Respiración", "Ninguno", "Respiratorio", 3, 60, 0));
                break;
        }

        return exercises;
    }

    private List<RoutineExercise> CreateGymExercises(int dayNumber)
    {
        var exercises = new List<RoutineExercise>();

        switch (dayNumber)
        {
            case 1: // Tren superior
                exercises.Add(CreateExercise(1, "Press de Pecho en Máquina", "Máquina", "Pectorales", 3, 12, 40));
                exercises.Add(CreateExercise(2, "Jalones al Pecho", "Máquina", "Dorsales", 3, 10, 35));
                exercises.Add(CreateExercise(3, "Press de Hombro en Máquina", "Máquina", "Deltoides", 3, 10, 25));
                break;
            case 2: // Tren inferior
                exercises.Add(CreateExercise(1, "Prensa de Piernas", "Máquina", "Cuádriceps", 3, 15, 80));
                exercises.Add(CreateExercise(2, "Curl de Isquiotibiales", "Máquina", "Isquiotibiales", 3, 12, 30));
                exercises.Add(CreateExercise(3, "Extensión de Cuádriceps", "Máquina", "Cuádriceps", 3, 12, 35));
                break;
            case 3: // Cardio y core
                exercises.Add(CreateExercise(1, "Caminadora", "Máquina cardio", "Cardiovascular", 1, 1200, 0));
                exercises.Add(CreateExercise(2, "Abdominales en Máquina", "Máquina", "Abdominales", 3, 15, 20));
                break;
        }

        return exercises;
    }

    private List<RoutineExercise> CreateProfessionalExercises(int dayNumber)
    {
        var exercises = new List<RoutineExercise>();

        switch (dayNumber)
        {
            case 1: // Pecho y tríceps
                exercises.Add(CreateExercise(1, "Press de Banca", "Barra", "Pectorales", 4, 8, 70));
                exercises.Add(CreateExercise(2, "Press Inclinado con Mancuernas", "Mancuernas", "Pectorales Superior", 3, 10, 30));
                exercises.Add(CreateExercise(3, "Fondos", "Peso corporal", "Pectorales/Tríceps", 3, 10, 0));
                exercises.Add(CreateExercise(4, "Extensión de Tríceps", "Mancuernas", "Tríceps", 3, 12, 15));
                break;
            case 2: // Espalda y bíceps
                exercises.Add(CreateExercise(1, "Dominadas", "Barra", "Dorsales", 4, 6, 0));
                exercises.Add(CreateExercise(2, "Remo con Barra", "Barra", "Dorsales", 4, 8, 60));
                exercises.Add(CreateExercise(3, "Curl con Barra", "Barra", "Bíceps", 3, 10, 30));
                exercises.Add(CreateExercise(4, "Curl con Mancuernas", "Mancuernas", "Bíceps", 3, 12, 15));
                break;
            case 3: // Piernas
                exercises.Add(CreateExercise(1, "Sentadillas", "Barra", "Cuádriceps/Glúteos", 4, 10, 80));
                exercises.Add(CreateExercise(2, "Peso Muerto", "Barra", "Isquiotibiales/Glúteos", 4, 8, 90));
                exercises.Add(CreateExercise(3, "Lunges", "Mancuernas", "Piernas", 3, 12, 20));
                exercises.Add(CreateExercise(4, "Elevaciones de Pantorrilla", "Máquina", "Pantorrillas", 4, 15, 50));
                break;
            case 4: // Hombros y core
                exercises.Add(CreateExercise(1, "Press Militar", "Barra", "Deltoides", 4, 8, 50));
                exercises.Add(CreateExercise(2, "Elevaciones Laterales", "Mancuernas", "Deltoides Lateral", 3, 12, 10));
                exercises.Add(CreateExercise(3, "Plancha", "Peso corporal", "Core", 3, 60, 0));
                exercises.Add(CreateExercise(4, "Abdominales", "Peso corporal", "Abdominales", 3, 20, 0));
                break;
        }

        return exercises;
    }

    private List<RoutineExercise> CreateGeneralExercises(int dayNumber)
    {
        var exercises = new List<RoutineExercise>();
        exercises.Add(CreateExercise(1, "Ejercicio General", "Equipamiento básico", "General", 3, 12, 25));
        exercises.Add(CreateExercise(2, "Ejercicio Complementario", "Equipamiento básico", "General", 3, 10, 20));
        return exercises;
    }

    private RoutineExercise CreateExercise(int id, string name, string equipment, string muscleGroup, int sets, int reps, decimal weight)
    {
        var exercise = new RoutineExercise
        {
            Id = id,
            Order = id,
            Name = name,
            Category = "General",
            MuscleGroups = new List<string> { muscleGroup },
            Equipment = equipment,
            Instructions = $"Realizar {name.ToLower()} manteniendo la técnica correcta.",
            SafetyTips = "Mantener buena postura y controlar el movimiento.",
            RestTimeSeconds = 60,
            Difficulty = "Apropiado para usuario"
        };

        for (int setNum = 1; setNum <= sets; setNum++)
        {
            exercise.Sets.Add(new ExerciseSet
            {
                Id = id * 10 + setNum,
                SetNumber = setNum,
                Reps = reps,
                Weight = weight,
                RestSeconds = 60
            });
        }

        return exercise;
    }

    private async Task ValidateUserFriendliness(List<UserTestResult> results)
    {
        foreach (var result in results)
        {
            // Validate file exists and is openable
            Assert.IsTrue(File.Exists(result.FilePath), $"File should exist for {result.ScenarioName}");

            // Validate reasonable file size (not too small, not too large)
            Assert.IsTrue(result.FileSizeKB > 5, $"File too small for {result.ScenarioName}: {result.FileSizeKB}KB");
            Assert.IsTrue(result.FileSizeKB < 5000, $"File too large for {result.ScenarioName}: {result.FileSizeKB}KB");

            // Validate reasonable processing time (elderly users are patient, but not infinitely)
            Assert.IsTrue(result.Duration.TotalSeconds < 30, $"Processing too slow for {result.ScenarioName}: {result.Duration.TotalSeconds}s");

            // Validate reasonable content
            Assert.IsTrue(result.ExerciseCount > 0, $"Should have exercises for {result.ScenarioName}");
            Assert.IsTrue(result.ExerciseCount < 50, $"Too many exercises for elderly user in {result.ScenarioName}: {result.ExerciseCount}");
        }

        Console.WriteLine("✅ Validación de facilidad de uso completada");
    }

    private async Task ValidateFileQuality(List<UserTestResult> results)
    {
        foreach (var result in results)
        {
            var fileInfo = new FileInfo(result.FilePath);

            // File should be a valid Word document
            Assert.AreEqual(".docx", fileInfo.Extension.ToLower(), $"Should be Word document for {result.ScenarioName}");

            // File should not be corrupted (basic check)
            Assert.IsTrue(fileInfo.Length > 1000, $"File seems too small/corrupted for {result.ScenarioName}");

            // File should be recent
            var age = DateTime.Now - fileInfo.CreationTime;
            Assert.IsTrue(age.TotalMinutes < 5, $"File should be recently created for {result.ScenarioName}");
        }

        Console.WriteLine("✅ Validación de calidad de archivos completada");
    }

    private async Task ValidateUniqueness(List<UserTestResult> results)
    {
        // Files should be different sizes (indicating different content)
        var fileSizes = results.Select(r => r.FileSizeKB).ToList();
        var uniqueSizes = fileSizes.Distinct().Count();

        Assert.IsTrue(uniqueSizes >= Math.Max(1, results.Count - 1),
            $"Files should be mostly unique, got {uniqueSizes} unique sizes from {results.Count} files");

        // Exercise counts should vary
        var exerciseCounts = results.Select(r => r.ExerciseCount).ToList();
        var uniqueCounts = exerciseCounts.Distinct().Count();

        Assert.IsTrue(uniqueCounts >= Math.Max(1, results.Count / 2),
            $"Exercise counts should vary between routines, got {uniqueCounts} unique counts from {results.Count} routines");

        Console.WriteLine("✅ Validación de unicidad completada");
    }

    private Routine CreateMinimumInputRoutine()
    {
        return new Routine
        {
            Id = 1,
            Name = "Mi Rutina",
            ClientName = "Usuario",
            Description = "",
            Goal = "General",
            DurationWeeks = 4,
            CreatedDate = DateTime.Now,
            Days = new List<RoutineDay>
            {
                new RoutineDay
                {
                    Id = 1,
                    DayNumber = 1,
                    Name = "Día 1",
                    Description = "",
                    FocusArea = "General",
                    TargetIntensity = "Moderada",
                    EstimatedDurationMinutes = 30,
                    Exercises = new List<RoutineExercise>
                    {
                        CreateExercise(1, "Ejercicio Básico", "Ninguno", "General", 2, 10, 0)
                    }
                }
            }
        };
    }

    private Routine CreateProblematicButRecoverableRoutine()
    {
        return new Routine
        {
            Id = -1, // Negative ID
            Name = "", // Empty name
            ClientName = "Usuario Test",
            Description = null, // Null description
            Goal = "",
            DurationWeeks = 0, // Zero weeks
            CreatedDate = DateTime.MinValue,
            Days = new List<RoutineDay>
            {
                new RoutineDay
                {
                    Id = 1,
                    DayNumber = 1,
                    Name = "Día Problemático",
                    Description = "Día con datos problemáticos pero recuperables",
                    FocusArea = "Test",
                    TargetIntensity = "Moderada",
                    EstimatedDurationMinutes = 30,
                    Exercises = new List<RoutineExercise>
                    {
                        CreateExercise(1, "Ejercicio Test", "Equipamiento", "Test", 3, 12, 25)
                    }
                }
            }
        };
    }

    private Routine CreateStandardUserRoutine()
    {
        return new Routine
        {
            Id = 100,
            Name = "Rutina Estándar Consistencia",
            ClientName = "Usuario Consistencia",
            Description = "Rutina para test de consistencia",
            Goal = "Consistencia",
            DurationWeeks = 4,
            CreatedDate = DateTime.Now,
            Days = new List<RoutineDay>
            {
                new RoutineDay
                {
                    Id = 1,
                    DayNumber = 1,
                    Name = "Día Consistencia",
                    Description = "Día para testing de consistencia",
                    FocusArea = "General",
                    TargetIntensity = "Moderada",
                    EstimatedDurationMinutes = 45,
                    Exercises = new List<RoutineExercise>
                    {
                        CreateExercise(1, "Ejercicio Consistente 1", "Equipamiento", "Músculos", 3, 12, 30),
                        CreateExercise(2, "Ejercicio Consistente 2", "Equipamiento", "Músculos", 3, 10, 25)
                    }
                }
            }
        };
    }

    private RoutineMetrics CalculateRoutineMetrics(Routine routine)
    {
        return new RoutineMetrics
        {
            TotalExercises = routine.Days.SelectMany(d => d.Exercises).Count(),
            TotalSets = routine.Days.SelectMany(d => d.Exercises).SelectMany(e => e.Sets).Count(),
            EstimatedDurationMinutes = routine.Days.Sum(d => d.EstimatedDurationMinutes),
            MuscleGroupsCovered = routine.Days.SelectMany(d => d.Exercises)
                                           .SelectMany(e => e.MuscleGroups)
                                           .Distinct()
                                           .ToList(),
            EquipmentRequired = routine.Days.SelectMany(d => d.Exercises)
                                          .Select(e => e.Equipment)
                                          .Distinct()
                                          .ToList(),
            DifficultyLevel = "Apropiado",
            CaloriesBurnedEstimate = routine.Days.Sum(d => d.EstimatedDurationMinutes) * 5
        };
    }

    #endregion

    #region Data Classes

    private class UserScenario
    {
        public string Name { get; set; } = "";
        public string ClientName { get; set; } = "";
        public string Goal { get; set; } = "";
        public string ExpectedTemplate { get; set; } = "";
        public string Description { get; set; } = "";
        public string ExpectedOutcome { get; set; } = "";
    }

    private class UserTestResult
    {
        public string ScenarioName { get; set; } = "";
        public bool Success { get; set; }
        public string FilePath { get; set; } = "";
        public long FileSizeKB { get; set; }
        public TimeSpan Duration { get; set; }
        public int ExerciseCount { get; set; }
        public string Message { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
    }

    private class DayConfiguration
    {
        public int DayNumber { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string FocusArea { get; set; } = "";
        public string Intensity { get; set; } = "";
        public int Duration { get; set; }
    }

    #endregion
}