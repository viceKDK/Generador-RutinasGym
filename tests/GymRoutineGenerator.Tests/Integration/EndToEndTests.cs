using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using GymRoutineGenerator.Core.Models.Routines;
using GymRoutineGenerator.Core.Services.Documents;
using GymRoutineGenerator.Infrastructure.Documents;
using System.Diagnostics;
using System.Linq;

namespace GymRoutineGenerator.Tests.Integration;

[TestClass]
public class EndToEndTests
{
    private IExportService _exportService;
    private string _testOutputDirectory;
    private WordDocumentService _wordService;
    private TemplateManagerService _templateService;

    [TestInitialize]
    public void Setup()
    {
        _testOutputDirectory = Path.Combine(Path.GetTempPath(), "GymRoutineTests", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
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
                Directory.Delete(_testOutputDirectory, true);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Cleanup failed: {ex.Message}");
        }
    }

    [TestMethod]
    [Description("Test completo: creación de rutina → exportación a Word → verificación del archivo")]
    public async Task EndToEnd_CompleteWorkflow_ShouldGenerateValidWordDocument()
    {
        // Arrange: Crear rutina realista
        var routine = CreateComprehensiveTestRoutine();
        var options = new ExportOptions
        {
            OutputPath = _testOutputDirectory,
            AutoOpenAfterExport = false,
            OverwriteExisting = true,
            CreateBackup = false
        };

        // Act: Exportar rutina
        var result = await _exportService.ExportRoutineToWordAsync(routine, "professional", options);

        // Assert: Verificar resultado
        Assert.IsTrue(result.Success, $"Export failed: {result.ErrorMessage}");
        Assert.IsNotNull(result.FilePath);
        Assert.IsTrue(File.Exists(result.FilePath), "Generated file does not exist");

        // Verificar propiedades del archivo
        var fileInfo = new FileInfo(result.FilePath);
        Assert.IsTrue(fileInfo.Length > 10000, "File too small, likely corrupted");
        Assert.AreEqual(".docx", fileInfo.Extension.ToLower());

        // Verificar métricas del resultado
        Assert.IsTrue(result.ExerciseCount > 0, "No exercises were included");
        Assert.IsTrue(result.ExportDuration.TotalSeconds < 30, "Export took too long");
        Assert.IsTrue(result.FileSizeBytes > 0, "File size not reported");

        Console.WriteLine($"✅ E2E Test Passed:");
        Console.WriteLine($"   📄 File: {Path.GetFileName(result.FilePath)}");
        Console.WriteLine($"   📏 Size: {result.FileSizeBytes / 1024:N0} KB");
        Console.WriteLine($"   💪 Exercises: {result.ExerciseCount}");
        Console.WriteLine($"   ⏱️ Duration: {result.ExportDuration.TotalSeconds:F1}s");
    }

    [TestMethod]
    [Description("Test de flujo completo con todas las plantillas disponibles")]
    public async Task EndToEnd_AllTemplates_ShouldGenerateSuccessfully()
    {
        // Arrange
        var routine = CreateStandardTestRoutine();
        var templates = new[] { "basic", "standard", "professional", "gym", "rehabilitation" };
        var results = new List<ExportResult>();

        // Act: Probar cada plantilla
        foreach (var template in templates)
        {
            var options = new ExportOptions
            {
                OutputPath = _testOutputDirectory,
                AutoOpenAfterExport = false,
                OverwriteExisting = true
            };

            var result = await _exportService.ExportRoutineToWordAsync(routine, template, options);
            results.Add(result);

            // Assert individual
            Assert.IsTrue(result.Success, $"Template {template} failed: {result.ErrorMessage}");
            Assert.IsTrue(File.Exists(result.FilePath), $"File not created for template {template}");
        }

        // Assert general
        Assert.AreEqual(templates.Length, results.Count);
        Assert.IsTrue(results.All(r => r.Success), "Not all templates succeeded");

        Console.WriteLine($"✅ All Templates Test Passed:");
        foreach (var (template, result) in templates.Zip(results, (t, r) => (t, r)))
        {
            Console.WriteLine($"   📋 {template}: {result.FileSizeBytes / 1024:N0} KB, {result.ExerciseCount} exercises");
        }
    }

    [TestMethod]
    [Description("Test de rutinas multi-día con diferentes configuraciones")]
    public async Task EndToEnd_MultiDayRoutines_AllParameterCombinations()
    {
        // Arrange: Diferentes combinaciones de parámetros
        var testCases = new[]
        {
            new { ClientName = "Ana García", Goal = "Fuerza", Days = 3, Weeks = 4, Template = "professional" },
            new { ClientName = "Carlos López", Goal = "Resistencia", Days = 5, Weeks = 6, Template = "gym" },
            new { ClientName = "María Rodríguez", Goal = "Flexibilidad", Days = 2, Weeks = 2, Template = "basic" },
            new { ClientName = "José Martín", Goal = "Rehabilitación", Days = 4, Weeks = 8, Template = "rehabilitation" },
            new { ClientName = "Laura Hernández", Goal = "General", Days = 6, Weeks = 12, Template = "standard" }
        };

        var successCount = 0;

        // Act & Assert
        foreach (var testCase in testCases)
        {
            var routine = CreateParameterizedRoutine(
                testCase.ClientName,
                testCase.Goal,
                testCase.Days,
                testCase.Weeks
            );

            var options = new ExportOptions
            {
                OutputPath = _testOutputDirectory,
                AutoOpenAfterExport = false,
                OverwriteExisting = true
            };

            var result = await _exportService.ExportRoutineToWordAsync(routine, testCase.Template, options);

            Assert.IsTrue(result.Success,
                $"Failed for {testCase.ClientName} - {testCase.Goal}: {result.ErrorMessage}");
            Assert.AreEqual(testCase.Days, routine.Days.Count,
                $"Day count mismatch for {testCase.ClientName}");

            successCount++;

            Console.WriteLine($"   ✅ {testCase.ClientName} ({testCase.Goal}): {testCase.Days} días, {testCase.Weeks} semanas");
        }

        Assert.AreEqual(testCases.Length, successCount, "Not all test cases passed");
        Console.WriteLine($"✅ Multi-Day Routines Test: {successCount}/{testCases.Length} passed");
    }

    [TestMethod]
    [Description("Test de progreso en tiempo real durante exportación")]
    public async Task EndToEnd_ProgressReporting_ShouldReportAccurateProgress()
    {
        // Arrange
        var routine = CreateLargeTestRoutine(); // Rutina grande para ver progreso
        var progressReports = new List<ExportProgress>();

        var progress = new Progress<ExportProgress>(p =>
        {
            progressReports.Add(new ExportProgress
            {
                PercentComplete = p.PercentComplete,
                CurrentOperation = p.CurrentOperation,
                CurrentStep = p.CurrentStep,
                TotalSteps = p.TotalSteps
            });
        });

        var options = new ExportOptions
        {
            OutputPath = _testOutputDirectory,
            AutoOpenAfterExport = false
        };

        // Act
        var result = await _exportService.ExportRoutineToWordAsync(routine, "professional", options, progress);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(progressReports.Count > 0, "No progress was reported");

        // Verificar progreso lógico
        var finalProgress = progressReports.Last();
        Assert.AreEqual(100, finalProgress.PercentComplete, "Final progress should be 100%");

        // Verificar que el progreso es incremental
        for (int i = 1; i < progressReports.Count; i++)
        {
            Assert.IsTrue(progressReports[i].PercentComplete >= progressReports[i-1].PercentComplete,
                "Progress should be monotonically increasing");
        }

        Console.WriteLine($"✅ Progress Reporting Test:");
        Console.WriteLine($"   📊 Progress reports: {progressReports.Count}");
        Console.WriteLine($"   🎯 Final progress: {finalProgress.PercentComplete}%");
        Console.WriteLine($"   📋 Steps: {finalProgress.CurrentStep}/{finalProgress.TotalSteps}");
    }

    [TestMethod]
    [Description("Test de validación final: 5 rutinas diferentes exitosas")]
    public async Task EndToEnd_FinalValidation_FiveDifferentRoutinesSuccessfully()
    {
        // Arrange: 5 rutinas completamente diferentes (simulando el test de "tu madre")
        var testRoutines = new[]
        {
            new { Name = "Principiante Casa", Client = "María Beginner", Goal = "General", Template = "basic" },
            new { Name = "Atleta Gimnasio", Client = "Carlos Athlete", Goal = "Fuerza", Template = "professional" },
            new { Name = "Senior Suave", Client = "Ana Senior", Goal = "Flexibilidad", Template = "rehabilitation" },
            new { Name = "Runner Resistencia", Client = "Luis Runner", Goal = "Resistencia", Template = "gym" },
            new { Name = "Oficinista Activo", Client = "Sofia Office", Goal = "General", Template = "standard" }
        };

        var results = new List<(string Name, bool Success, string FilePath, string Error)>();

        // Act: Generar cada rutina independientemente
        foreach (var testCase in testRoutines)
        {
            try
            {
                var routine = CreateRoutineByProfile(testCase.Goal, testCase.Client);
                routine.Name = testCase.Name;

                var options = new ExportOptions
                {
                    OutputPath = _testOutputDirectory,
                    AutoOpenAfterExport = false,
                    OverwriteExisting = true
                };

                var result = await _exportService.ExportRoutineToWordAsync(routine, testCase.Template, options);

                results.Add((testCase.Name, result.Success, result.FilePath, result.ErrorMessage));

                // Verificar que el archivo es válido
                if (result.Success && File.Exists(result.FilePath))
                {
                    var fileInfo = new FileInfo(result.FilePath);
                    Assert.IsTrue(fileInfo.Length > 5000, $"File too small for {testCase.Name}");
                }
            }
            catch (Exception ex)
            {
                results.Add((testCase.Name, false, "", ex.Message));
            }
        }

        // Assert: Todas deben ser exitosas
        var successfulResults = results.Where(r => r.Success).ToList();

        Assert.AreEqual(5, successfulResults.Count,
            $"Expected 5 successful routines, got {successfulResults.Count}. " +
            $"Failures: {string.Join(", ", results.Where(r => !r.Success).Select(r => $"{r.Name}: {r.Error}"))}");

        // Verificar que todas las rutinas son únicas
        var uniqueFiles = successfulResults.Select(r => new FileInfo(r.FilePath).Length).Distinct().Count();
        Assert.IsTrue(uniqueFiles >= 3, "Routines should be significantly different (file sizes vary)");

        Console.WriteLine($"🎉 FINAL VALIDATION PASSED - 5 Rutinas Generadas Exitosamente:");
        foreach (var (name, success, filePath, error) in results)
        {
            if (success)
            {
                var size = new FileInfo(filePath).Length / 1024;
                Console.WriteLine($"   ✅ {name}: {size:N0} KB - {Path.GetFileName(filePath)}");
            }
            else
            {
                Console.WriteLine($"   ❌ {name}: {error}");
            }
        }
    }

    #region Helper Methods

    private Routine CreateComprehensiveTestRoutine()
    {
        var routine = new Routine
        {
            Id = 1001,
            Name = "Rutina Test Completa E2E",
            ClientName = "Usuario Test Completo",
            Description = "Rutina comprensiva para testing end-to-end con múltiples ejercicios y configuraciones avanzadas",
            Goal = "Testing Completo",
            DurationWeeks = 6,
            CreatedDate = DateTime.Now
        };

        // Crear 4 días de entrenamiento con variedad
        for (int dayNum = 1; dayNum <= 4; dayNum++)
        {
            var day = new RoutineDay
            {
                Id = dayNum,
                DayNumber = dayNum,
                Name = $"Día {dayNum} - Testing {GetDayFocus(dayNum)}",
                Description = $"Sesión de testing enfocada en {GetDayFocus(dayNum).ToLower()}",
                FocusArea = GetDayFocus(dayNum),
                TargetIntensity = GetIntensityForDay(dayNum),
                EstimatedDurationMinutes = 60 + (dayNum * 15)
            };

            // Agregar 6-8 ejercicios por día
            var exerciseCount = 6 + (dayNum % 3);
            for (int exNum = 1; exNum <= exerciseCount; exNum++)
            {
                var exercise = CreateTestExercise(dayNum, exNum);
                day.Exercises.Add(exercise);
            }

            routine.Days.Add(day);
        }

        routine.Metrics = CalculateTestMetrics(routine);
        return routine;
    }

    private Routine CreateStandardTestRoutine()
    {
        return new Routine
        {
            Id = 2001,
            Name = "Rutina Estándar Test",
            ClientName = "Cliente Estándar",
            Description = "Rutina estándar para probar todas las plantillas",
            Goal = "Fuerza General",
            DurationWeeks = 4,
            CreatedDate = DateTime.Now,
            Days = new List<RoutineDay>
            {
                CreateStandardTestDay(1, "Pecho y Tríceps"),
                CreateStandardTestDay(2, "Espalda y Bíceps"),
                CreateStandardTestDay(3, "Piernas y Glúteos")
            }
        };
    }

    private Routine CreateLargeTestRoutine()
    {
        var routine = new Routine
        {
            Id = 3001,
            Name = "Rutina Grande Test Performance",
            ClientName = "Cliente Performance Test",
            Description = "Rutina grande para testing de performance y progreso",
            Goal = "Testing Performance",
            DurationWeeks = 8,
            CreatedDate = DateTime.Now
        };

        // Crear 6 días con muchos ejercicios cada uno
        for (int dayNum = 1; dayNum <= 6; dayNum++)
        {
            var day = new RoutineDay
            {
                Id = dayNum,
                DayNumber = dayNum,
                Name = $"Día {dayNum} - Performance Test",
                Description = $"Día {dayNum} con múltiples ejercicios para testing de performance",
                FocusArea = GetDayFocus(dayNum),
                TargetIntensity = "Alta",
                EstimatedDurationMinutes = 90
            };

            // 12 ejercicios por día = 72 ejercicios total
            for (int exNum = 1; exNum <= 12; exNum++)
            {
                var exercise = CreateTestExercise(dayNum, exNum);
                day.Exercises.Add(exercise);
            }

            routine.Days.Add(day);
        }

        routine.Metrics = CalculateTestMetrics(routine);
        return routine;
    }

    private Routine CreateParameterizedRoutine(string clientName, string goal, int days, int weeks)
    {
        var routine = new Routine
        {
            Id = new Random().Next(4000, 9999),
            Name = $"Rutina {goal} - {clientName}",
            ClientName = clientName,
            Description = $"Rutina personalizada de {goal.ToLower()} para {clientName} - {weeks} semanas, {days} días",
            Goal = goal,
            DurationWeeks = weeks,
            CreatedDate = DateTime.Now
        };

        for (int dayNum = 1; dayNum <= days; dayNum++)
        {
            var day = new RoutineDay
            {
                Id = dayNum,
                DayNumber = dayNum,
                Name = $"Día {dayNum} - {goal}",
                Description = $"Sesión {dayNum} enfocada en {goal.ToLower()}",
                FocusArea = GetFocusAreaForGoal(goal, dayNum),
                TargetIntensity = GetIntensityForGoal(goal),
                EstimatedDurationMinutes = GetDurationForGoal(goal)
            };

            // Ejercicios basados en el objetivo
            var exerciseCount = GetExerciseCountForGoal(goal);
            for (int exNum = 1; exNum <= exerciseCount; exNum++)
            {
                var exercise = CreateExerciseForGoal(goal, dayNum, exNum);
                day.Exercises.Add(exercise);
            }

            routine.Days.Add(day);
        }

        routine.Metrics = CalculateTestMetrics(routine);
        return routine;
    }

    private Routine CreateRoutineByProfile(string goal, string clientName)
    {
        // Crear rutinas específicas por perfil para el test final
        return goal.ToLower() switch
        {
            "general" => CreateGeneralProfileRoutine(clientName),
            "fuerza" => CreateStrengthProfileRoutine(clientName),
            "flexibilidad" => CreateFlexibilityProfileRoutine(clientName),
            "resistencia" => CreateEnduranceProfileRoutine(clientName),
            _ => CreateGeneralProfileRoutine(clientName)
        };
    }

    private Routine CreateGeneralProfileRoutine(string clientName)
    {
        return CreateParameterizedRoutine(clientName, "General", 3, 4);
    }

    private Routine CreateStrengthProfileRoutine(string clientName)
    {
        return CreateParameterizedRoutine(clientName, "Fuerza", 4, 6);
    }

    private Routine CreateFlexibilityProfileRoutine(string clientName)
    {
        return CreateParameterizedRoutine(clientName, "Flexibilidad", 2, 3);
    }

    private Routine CreateEnduranceProfileRoutine(string clientName)
    {
        return CreateParameterizedRoutine(clientName, "Resistencia", 5, 8);
    }

    private RoutineDay CreateStandardTestDay(int dayNumber, string focus)
    {
        var day = new RoutineDay
        {
            Id = dayNumber,
            DayNumber = dayNumber,
            Name = $"Día {dayNumber} - {focus}",
            Description = $"Entrenamiento enfocado en {focus.ToLower()}",
            FocusArea = focus,
            TargetIntensity = "Moderada-Alta",
            EstimatedDurationMinutes = 70
        };

        for (int i = 1; i <= 5; i++)
        {
            day.Exercises.Add(CreateTestExercise(dayNumber, i));
        }

        return day;
    }

    private RoutineExercise CreateTestExercise(int dayNumber, int exerciseNumber)
    {
        var exerciseNames = new[]
        {
            "Press de Banca", "Sentadillas", "Peso Muerto", "Press Militar",
            "Dominadas", "Remo con Barra", "Curl de Bíceps", "Extensiones de Tríceps",
            "Elevaciones Laterales", "Fondos", "Plancha", "Abdominales"
        };

        var exerciseName = exerciseNames[(dayNumber * exerciseNumber) % exerciseNames.Length];

        var exercise = new RoutineExercise
        {
            Id = (dayNumber * 100) + exerciseNumber,
            Order = exerciseNumber,
            Name = exerciseName,
            Category = "Fuerza",
            MuscleGroups = GetMuscleGroupsForExercise(exerciseName),
            Equipment = "Equipamiento de gimnasio",
            Instructions = $"Ejecutar {exerciseName.ToLower()} con técnica correcta, controlando el movimiento en ambas fases.",
            SafetyTips = "Mantener postura correcta, calentar adecuadamente, detenerse si hay dolor.",
            RestTimeSeconds = 90,
            Difficulty = "Intermedio"
        };

        // Crear sets
        for (int setNum = 1; setNum <= 4; setNum++)
        {
            exercise.Sets.Add(new ExerciseSet
            {
                Id = exercise.Id * 10 + setNum,
                SetNumber = setNum,
                Reps = 12 - setNum, // 12, 11, 10, 9 (pirámide descendente)
                Weight = 50 + (setNum * 5), // 50, 55, 60, 65
                RestSeconds = exercise.RestTimeSeconds
            });
        }

        return exercise;
    }

    private RoutineExercise CreateExerciseForGoal(string goal, int dayNumber, int exerciseNumber)
    {
        var exerciseData = goal.ToLower() switch
        {
            "fuerza" => new { Name = GetStrengthExercise(exerciseNumber), Category = "Fuerza", Reps = 8, Weight = 70m },
            "resistencia" => new { Name = GetEnduranceExercise(exerciseNumber), Category = "Cardio", Reps = 20, Weight = 0m },
            "flexibilidad" => new { Name = GetFlexibilityExercise(exerciseNumber), Category = "Flexibilidad", Reps = 1, Weight = 0m },
            _ => new { Name = GetGeneralExercise(exerciseNumber), Category = "General", Reps = 12, Weight = 40m }
        };

        var exercise = new RoutineExercise
        {
            Id = (dayNumber * 100) + exerciseNumber,
            Order = exerciseNumber,
            Name = exerciseData.Name,
            Category = exerciseData.Category,
            MuscleGroups = GetMuscleGroupsForExercise(exerciseData.Name),
            Equipment = GetEquipmentForExercise(exerciseData.Name),
            Instructions = $"Realizar {exerciseData.Name.ToLower()} siguiendo la técnica apropiada.",
            SafetyTips = "Calentar antes de ejercitarse. Parar si hay dolor.",
            RestTimeSeconds = GetRestTimeForGoal(goal),
            Difficulty = GetDifficultyForGoal(goal)
        };

        // Crear sets apropiados para el objetivo
        var setCount = goal.ToLower() == "flexibilidad" ? 3 : 4;
        for (int setNum = 1; setNum <= setCount; setNum++)
        {
            exercise.Sets.Add(new ExerciseSet
            {
                Id = exercise.Id * 10 + setNum,
                SetNumber = setNum,
                Reps = exerciseData.Reps,
                Weight = exerciseData.Weight,
                RestSeconds = exercise.RestTimeSeconds
            });
        }

        return exercise;
    }

    #region Data Helper Methods

    private string GetDayFocus(int dayNumber)
    {
        return dayNumber switch
        {
            1 => "Pecho y Tríceps",
            2 => "Espalda y Bíceps",
            3 => "Piernas y Glúteos",
            4 => "Hombros y Core",
            5 => "Cuerpo Completo",
            6 => "Funcional",
            _ => "General"
        };
    }

    private string GetIntensityForDay(int dayNumber)
    {
        return dayNumber switch
        {
            1 or 3 => "Alta",
            2 or 4 => "Moderada-Alta",
            _ => "Moderada"
        };
    }

    private string GetFocusAreaForGoal(string goal, int dayNumber)
    {
        return goal.ToLower() switch
        {
            "fuerza" => GetDayFocus(dayNumber),
            "resistencia" => "Cardiovascular",
            "flexibilidad" => "Movilidad",
            "rehabilitacion" => "Recuperación",
            _ => "General"
        };
    }

    private string GetIntensityForGoal(string goal)
    {
        return goal.ToLower() switch
        {
            "fuerza" => "Alta",
            "resistencia" => "Moderada-Alta",
            "flexibilidad" => "Baja",
            "rehabilitacion" => "Muy Baja",
            _ => "Moderada"
        };
    }

    private int GetDurationForGoal(string goal)
    {
        return goal.ToLower() switch
        {
            "fuerza" => 80,
            "resistencia" => 60,
            "flexibilidad" => 45,
            "rehabilitacion" => 30,
            _ => 65
        };
    }

    private int GetExerciseCountForGoal(string goal)
    {
        return goal.ToLower() switch
        {
            "fuerza" => 6,
            "resistencia" => 8,
            "flexibilidad" => 10,
            "rehabilitacion" => 4,
            _ => 6
        };
    }

    private int GetRestTimeForGoal(string goal)
    {
        return goal.ToLower() switch
        {
            "fuerza" => 120,
            "resistencia" => 45,
            "flexibilidad" => 30,
            "rehabilitacion" => 60,
            _ => 75
        };
    }

    private string GetDifficultyForGoal(string goal)
    {
        return goal.ToLower() switch
        {
            "fuerza" => "Intermedio-Avanzado",
            "resistencia" => "Intermedio",
            "flexibilidad" => "Principiante",
            "rehabilitacion" => "Principiante",
            _ => "Intermedio"
        };
    }

    private string GetStrengthExercise(int num)
    {
        var exercises = new[] { "Press de Banca", "Sentadillas", "Peso Muerto", "Press Militar", "Dominadas", "Remo con Barra" };
        return exercises[num % exercises.Length];
    }

    private string GetEnduranceExercise(int num)
    {
        var exercises = new[] { "Burpees", "Mountain Climbers", "Jumping Jacks", "High Knees", "Planchas", "Sprint Intervals", "Bike Intervals", "Running" };
        return exercises[num % exercises.Length];
    }

    private string GetFlexibilityExercise(int num)
    {
        var exercises = new[] { "Estiramiento Isquiotibiales", "Estiramiento Cuádriceps", "Estiramiento Hombros", "Gato-Camello", "Postura del Niño", "Estiramiento Pantorrillas" };
        return exercises[num % exercises.Length];
    }

    private string GetGeneralExercise(int num)
    {
        var exercises = new[] { "Sentadillas", "Flexiones", "Plancha", "Lunges", "Abdominales", "Step-ups" };
        return exercises[num % exercises.Length];
    }

    private List<string> GetMuscleGroupsForExercise(string exerciseName)
    {
        return exerciseName.ToLower() switch
        {
            var e when e.Contains("press") && e.Contains("banca") => new List<string> { "Pectorales", "Tríceps", "Deltoides" },
            var e when e.Contains("sentadilla") => new List<string> { "Cuádriceps", "Glúteos", "Core" },
            var e when e.Contains("peso muerto") => new List<string> { "Isquiotibiales", "Glúteos", "Espalda Baja" },
            var e when e.Contains("dominada") => new List<string> { "Dorsales", "Bíceps", "Antebrazos" },
            var e when e.Contains("plancha") => new List<string> { "Core", "Hombros", "Pecho" },
            var e when e.Contains("curl") => new List<string> { "Bíceps", "Antebrazos" },
            _ => new List<string> { "Múltiples grupos musculares" }
        };
    }

    private string GetEquipmentForExercise(string exerciseName)
    {
        return exerciseName.ToLower() switch
        {
            var e when e.Contains("press banca") => "Barra y banco",
            var e when e.Contains("mancuerna") => "Mancuernas",
            var e when e.Contains("barra") => "Barra olímpica",
            var e when e.Contains("dominada") => "Barra de dominadas",
            var e when e.Contains("peso corporal") || e.Contains("plancha") || e.Contains("flexion") => "Peso corporal",
            var e when e.Contains("estiramiento") => "Colchoneta",
            _ => "Equipamiento básico de gimnasio"
        };
    }

    private RoutineMetrics CalculateTestMetrics(Routine routine)
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
            DifficultyLevel = routine.Days.FirstOrDefault()?.Exercises.FirstOrDefault()?.Difficulty ?? "Intermedio",
            CaloriesBurnedEstimate = routine.Days.Sum(d => d.EstimatedDurationMinutes) * 8 // 8 cal/min estimate
        };
    }

    #endregion

    #endregion
}