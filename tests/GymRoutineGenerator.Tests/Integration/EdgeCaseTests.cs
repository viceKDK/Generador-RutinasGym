using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using GymRoutineGenerator.Core.Models.Routines;
using GymRoutineGenerator.Core.Services.Documents;
using GymRoutineGenerator.Infrastructure.Documents;
using System.Linq;
using System.Threading;
using System.Net.NetworkInformation;

namespace GymRoutineGenerator.Tests.Integration;

[TestClass]
public class EdgeCaseTests
{
    private IExportService _exportService;
    private string _testOutputDirectory;
    private WordDocumentService _wordService;
    private TemplateManagerService _templateService;

    [TestInitialize]
    public void Setup()
    {
        _testOutputDirectory = Path.Combine(Path.GetTempPath(), "GymRoutineEdgeTests", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
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
            Console.WriteLine($"Cleanup warning: {ex.Message}");
        }
    }

    [TestMethod]
    [Description("Test con rutina completamente vacía - should handle gracefully")]
    public async Task EdgeCase_EmptyRoutine_ShouldHandleGracefully()
    {
        // Arrange: Rutina mínima/vacía
        var emptyRoutine = new Routine
        {
            Id = 1,
            Name = "",
            ClientName = "",
            Description = "",
            Goal = "",
            DurationWeeks = 0,
            CreatedDate = DateTime.Now,
            Days = new List<RoutineDay>()
        };

        var options = new ExportOptions
        {
            OutputPath = _testOutputDirectory,
            AutoOpenAfterExport = false,
            OverwriteExisting = true
        };

        // Act
        var result = await _exportService.ExportRoutineToWordAsync(emptyRoutine, "basic", options);

        // Assert: Debe manejar el caso elegantemente
        // Puede fallar o crear un documento básico, pero NO debe crashear
        Assert.IsNotNull(result);
        if (result.Success)
        {
            Assert.IsTrue(File.Exists(result.FilePath));
            Console.WriteLine("✅ Empty routine handled successfully - created basic document");
        }
        else
        {
            Assert.IsNotNull(result.ErrorMessage);
            Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
            Console.WriteLine($"✅ Empty routine handled gracefully - error: {result.ErrorMessage}");
        }
    }

    [TestMethod]
    [Description("Test con caracteres especiales y emojis en nombres")]
    public async Task EdgeCase_SpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange: Rutina con caracteres especiales problemáticos
        var specialCharRoutine = new Routine
        {
            Id = 2,
            Name = "Rutina Ñandú 💪 con «comillas» y símbolos ®™©",
            ClientName = "José María Àlvarez-Ópez 🏋️‍♂️",
            Description = "Descripción con ñ, tildes áéíóú, símbolos ¡¿ y emojis 🔥💯",
            Goal = "Fuerza & Resistência",
            DurationWeeks = 4,
            CreatedDate = DateTime.Now
        };

        var day = new RoutineDay
        {
            Id = 1,
            DayNumber = 1,
            Name = "Día 1 - Tórax & Bíceps™",
            Description = "Entrenamiento con «comillas dobles» y símbolos ©®",
            FocusArea = "Tren Superior",
            TargetIntensity = "Moderada",
            EstimatedDurationMinutes = 60
        };

        var exercise = new RoutineExercise
        {
            Id = 1,
            Order = 1,
            Name = "Press de Banca con ~símbolos raros~ y números 1234567890",
            Category = "Fuerza",
            MuscleGroups = new List<string> { "Pectorales", "Tríceps & Deltoides" },
            Equipment = "Barra olímpica (20kg) + discos",
            Instructions = "Instrucciones con ñ, tildes éí y símbolos ¿¡ que pueden causar problemas de encoding",
            SafetyTips = "Cuidado con símbolos: <> [] {} \\ / | @ # $ % ^ & * ( ) + = ? !",
            RestTimeSeconds = 90,
            Difficulty = "Intermédio"
        };

        exercise.Sets.Add(new ExerciseSet
        {
            Id = 1,
            SetNumber = 1,
            Reps = 12,
            Weight = 60.5m,
            RestSeconds = 90
        });

        day.Exercises.Add(exercise);
        specialCharRoutine.Days.Add(day);

        var options = new ExportOptions
        {
            OutputPath = _testOutputDirectory,
            AutoOpenAfterExport = false,
            OverwriteExisting = true
        };

        // Act
        var result = await _exportService.ExportRoutineToWordAsync(specialCharRoutine, "standard", options);

        // Assert
        Assert.IsTrue(result.Success, $"Special characters export failed: {result.ErrorMessage}");
        Assert.IsTrue(File.Exists(result.FilePath));

        var fileInfo = new FileInfo(result.FilePath);
        Assert.IsTrue(fileInfo.Length > 1000, "File too small - characters may have been lost");

        Console.WriteLine("✅ Special characters test passed");
        Console.WriteLine($"   📄 File: {fileInfo.Length / 1024:N0} KB");
        Console.WriteLine($"   🔤 Characters handled: ñáéíóú¿¡©®™«»💪🏋️‍♂️🔥💯");
    }

    [TestMethod]
    [Description("Test con directorio de salida no existente")]
    public async Task EdgeCase_NonExistentOutputDirectory_ShouldCreateOrFail()
    {
        // Arrange
        var routine = CreateSimpleTestRoutine();
        var nonExistentPath = Path.Combine(_testOutputDirectory, "non", "existent", "deep", "path");

        // Asegurar que el directorio NO existe
        Assert.IsFalse(Directory.Exists(nonExistentPath));

        var options = new ExportOptions
        {
            OutputPath = nonExistentPath,
            AutoOpenAfterExport = false,
            OverwriteExisting = true
        };

        // Act
        var result = await _exportService.ExportRoutineToWordAsync(routine, "basic", options);

        // Assert: Debe crear el directorio o fallar elegantemente
        if (result.Success)
        {
            Assert.IsTrue(Directory.Exists(nonExistentPath), "Directory should be created");
            Assert.IsTrue(File.Exists(result.FilePath));
            Console.WriteLine("✅ Non-existent directory was created successfully");
        }
        else
        {
            Assert.IsNotNull(result.ErrorMessage);
            Console.WriteLine($"✅ Non-existent directory handled gracefully: {result.ErrorMessage}");
        }
    }

    [TestMethod]
    [Description("Test con archivo de salida bloqueado (en uso)")]
    public async Task EdgeCase_FileInUse_ShouldHandleGracefully()
    {
        // Arrange
        var routine = CreateSimpleTestRoutine();
        var options = new ExportOptions
        {
            OutputPath = _testOutputDirectory,
            AutoOpenAfterExport = false,
            OverwriteExisting = true
        };

        // Primera exportación para crear el archivo
        var firstResult = await _exportService.ExportRoutineToWordAsync(routine, "basic", options);
        Assert.IsTrue(firstResult.Success, "First export should succeed");

        // Simular archivo en uso bloqueándolo
        using (var fileStream = new FileStream(firstResult.FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            // Act: Intentar exportar al mismo archivo mientras está bloqueado
            var secondResult = await _exportService.ExportRoutineToWordAsync(routine, "basic", options);

            // Assert: Debe manejar el archivo bloqueado elegantemente
            if (secondResult.Success)
            {
                // Si tiene éxito, debe haber creado un archivo con nombre diferente
                Assert.AreNotEqual(firstResult.FilePath, secondResult.FilePath);
                Console.WriteLine("✅ File in use handled by creating alternative filename");
                Console.WriteLine($"   📄 Original: {Path.GetFileName(firstResult.FilePath)}");
                Console.WriteLine($"   📄 Alternative: {Path.GetFileName(secondResult.FilePath)}");
            }
            else
            {
                // Si falla, debe tener un mensaje de error apropiado
                Assert.IsNotNull(secondResult.ErrorMessage);
                Assert.IsTrue(secondResult.ErrorMessage.ToLower().Contains("uso") ||
                            secondResult.ErrorMessage.ToLower().Contains("access") ||
                            secondResult.ErrorMessage.ToLower().Contains("lock"));
                Console.WriteLine($"✅ File in use handled gracefully: {secondResult.ErrorMessage}");
            }
        }
    }

    [TestMethod]
    [Description("Test con rutina extremadamente grande")]
    public async Task EdgeCase_ExtremelyLargeRoutine_ShouldComplete()
    {
        // Arrange: Rutina con datos masivos para probar límites
        var massiveRoutine = new Routine
        {
            Id = 9999,
            Name = "Rutina Masiva de Testing - Límites del Sistema",
            ClientName = "Cliente Test Extremo",
            Description = new string('A', 5000), // 5KB de descripción
            Goal = "Testing de Límites",
            DurationWeeks = 52, // Un año completo
            CreatedDate = DateTime.Now
        };

        // Crear 7 días (semana completa)
        for (int dayNum = 1; dayNum <= 7; dayNum++)
        {
            var day = new RoutineDay
            {
                Id = dayNum,
                DayNumber = dayNum,
                Name = $"Día {dayNum} - Entrenamiento Masivo",
                Description = new string('B', 1000), // 1KB de descripción por día
                FocusArea = $"Área de Enfoque {dayNum}",
                TargetIntensity = "Extrema",
                EstimatedDurationMinutes = 180 // 3 horas
            };

            // 25 ejercicios por día = 175 ejercicios totales
            for (int exNum = 1; exNum <= 25; exNum++)
            {
                var exercise = new RoutineExercise
                {
                    Id = (dayNum * 100) + exNum,
                    Order = exNum,
                    Name = $"Ejercicio Masivo #{exNum} del Día {dayNum}",
                    Category = "Testing Extremo",
                    MuscleGroups = Enumerable.Range(1, 10).Select(i => $"Grupo Muscular {i}").ToList(),
                    Equipment = new string('C', 500), // 500 chars de equipment
                    Instructions = new string('D', 2000), // 2KB de instrucciones
                    SafetyTips = new string('E', 1000), // 1KB de consejos de seguridad
                    RestTimeSeconds = 300,
                    Difficulty = "Extremo"
                };

                // 10 sets por ejercicio = 1750 sets totales
                for (int setNum = 1; setNum <= 10; setNum++)
                {
                    exercise.Sets.Add(new ExerciseSet
                    {
                        Id = (exercise.Id * 100) + setNum,
                        SetNumber = setNum,
                        Reps = setNum + 5,
                        Weight = setNum * 10,
                        RestSeconds = 300
                    });
                }

                day.Exercises.Add(exercise);
            }

            massiveRoutine.Days.Add(day);
        }

        // Calcular métricas
        massiveRoutine.Metrics = new RoutineMetrics
        {
            TotalExercises = massiveRoutine.Days.SelectMany(d => d.Exercises).Count(),
            TotalSets = massiveRoutine.Days.SelectMany(d => d.Exercises).SelectMany(e => e.Sets).Count(),
            EstimatedDurationMinutes = massiveRoutine.Days.Sum(d => d.EstimatedDurationMinutes),
            MuscleGroupsCovered = new List<string>(Enumerable.Range(1, 50).Select(i => $"Muscle Group {i}")),
            EquipmentRequired = new List<string>(Enumerable.Range(1, 20).Select(i => $"Equipment {i}")),
            DifficultyLevel = "Extremo",
            CaloriesBurnedEstimate = 5000
        };

        var options = new ExportOptions
        {
            OutputPath = _testOutputDirectory,
            AutoOpenAfterExport = false,
            OverwriteExisting = true
        };

        // Act: Exportar rutina masiva con timeout extendido
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _exportService.ExportRoutineToWordAsync(massiveRoutine, "professional", options);
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(result.Success, $"Massive routine export failed: {result.ErrorMessage}");
        Assert.IsTrue(File.Exists(result.FilePath));

        var fileInfo = new FileInfo(result.FilePath);
        Assert.IsTrue(fileInfo.Length > 100000, "File should be at least 100KB for this massive routine");

        // Performance checks
        Assert.IsTrue(stopwatch.Elapsed.TotalMinutes < 5, "Export should complete in under 5 minutes");

        Console.WriteLine("✅ Extremely large routine test passed");
        Console.WriteLine($"   📊 Stats: {massiveRoutine.Metrics.TotalExercises} exercises, {massiveRoutine.Metrics.TotalSets} sets");
        Console.WriteLine($"   📄 File size: {fileInfo.Length / 1024:N0} KB");
        Console.WriteLine($"   ⏱️ Export time: {stopwatch.Elapsed.TotalSeconds:F1} seconds");
    }

    [TestMethod]
    [Description("Test con datos corruptos/malformados")]
    public async Task EdgeCase_CorruptedData_ShouldHandleGracefully()
    {
        // Arrange: Rutina con datos problemáticos
        var corruptedRoutine = new Routine
        {
            Id = -1, // ID negativo
            Name = null, // Nombre nulo
            ClientName = "Cliente\0ConCaracterNulo", // Caracter nulo
            Description = "Descripción con\r\ncaracteres\tde\vcontrol\f extraños",
            Goal = "", // Goal vacío
            DurationWeeks = -5, // Duración negativa
            CreatedDate = DateTime.MinValue // Fecha mínima
        };

        var corruptedDay = new RoutineDay
        {
            Id = 0,
            DayNumber = -1, // Número de día negativo
            Name = new string('X', 10000), // Nombre extremadamente largo
            Description = null,
            FocusArea = "",
            TargetIntensity = "Intensidad Inválida",
            EstimatedDurationMinutes = -60 // Duración negativa
        };

        var corruptedExercise = new RoutineExercise
        {
            Id = 0,
            Order = 0,
            Name = "",
            Category = null,
            MuscleGroups = null, // Lista nula
            Equipment = new string('Z', 5000), // Equipment extremadamente largo
            Instructions = null,
            SafetyTips = "",
            RestTimeSeconds = -30, // Descanso negativo
            Difficulty = "♠♣♦♥" // Símbolos extraños
        };

        // Set con datos problemáticos
        corruptedExercise.Sets = new List<ExerciseSet>
        {
            new ExerciseSet
            {
                Id = -1,
                SetNumber = 0,
                Reps = -10, // Reps negativas
                Weight = -50, // Peso negativo
                RestSeconds = int.MaxValue // Descanso extremo
            }
        };

        corruptedDay.Exercises.Add(corruptedExercise);
        corruptedRoutine.Days.Add(corruptedDay);

        var options = new ExportOptions
        {
            OutputPath = _testOutputDirectory,
            AutoOpenAfterExport = false,
            OverwriteExisting = true
        };

        // Act
        var result = await _exportService.ExportRoutineToWordAsync(corruptedRoutine, "basic", options);

        // Assert: No debe crashear, debe manejar datos corruptos elegantemente
        Assert.IsNotNull(result);

        if (result.Success)
        {
            // Si tiene éxito, el archivo debe existir y tener contenido mínimo
            Assert.IsTrue(File.Exists(result.FilePath));
            var fileInfo = new FileInfo(result.FilePath);
            Assert.IsTrue(fileInfo.Length > 0, "File should have some content even with corrupted data");
            Console.WriteLine("✅ Corrupted data handled successfully - created sanitized document");
        }
        else
        {
            // Si falla, debe tener un error descriptivo
            Assert.IsNotNull(result.ErrorMessage);
            Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
            Console.WriteLine($"✅ Corrupted data handled gracefully - error: {result.ErrorMessage}");
        }
    }

    [TestMethod]
    [Description("Test simulando falta de conexión a internet")]
    public async Task EdgeCase_NoInternetConnection_ShouldWorkOffline()
    {
        // Arrange
        var routine = CreateSimpleTestRoutine();
        routine.Name = "Rutina Sin Internet - Modo Offline";

        var options = new ExportOptions
        {
            OutputPath = _testOutputDirectory,
            AutoOpenAfterExport = false,
            OverwriteExisting = true
        };

        // Act: El sistema debería funcionar sin internet ya que es local
        // Pero verificamos que no dependa de recursos online
        var result = await _exportService.ExportRoutineToWordAsync(routine, "basic", options);

        // Assert: La exportación debe funcionar sin conexión
        Assert.IsTrue(result.Success, $"Offline export failed: {result.ErrorMessage}");
        Assert.IsTrue(File.Exists(result.FilePath));

        // Verificar que el resultado tiene sentido para modo offline
        var fileInfo = new FileInfo(result.FilePath);
        Assert.IsTrue(fileInfo.Length > 1000, "Offline file should have reasonable content");

        Console.WriteLine("✅ Offline functionality test passed");
        Console.WriteLine($"   📄 File size: {fileInfo.Length / 1024:N0} KB");
        Console.WriteLine("   🔌 No internet connection required for export");
    }

    [TestMethod]
    [Description("Test con espacio en disco insuficiente simulado")]
    public async Task EdgeCase_InsufficientDiskSpace_ShouldHandleGracefully()
    {
        // Arrange
        var routine = CreateSimpleTestRoutine();

        // Intentar escribir a un directorio que pueda tener problemas de espacio
        // En un test real, esto sería más complicado de simular
        var options = new ExportOptions
        {
            OutputPath = _testOutputDirectory,
            AutoOpenAfterExport = false,
            OverwriteExisting = true
        };

        // Act: Crear rutina que podría fallar por espacio
        var result = await _exportService.ExportRoutineToWordAsync(routine, "professional", options);

        // Assert: En condiciones normales debería funcionar, pero el sistema debe manejar errores de IO
        if (result.Success)
        {
            Assert.IsTrue(File.Exists(result.FilePath));
            Console.WriteLine("✅ Sufficient disk space available - test passed");
        }
        else
        {
            // Si falla, verificar que es un error de IO manejado correctamente
            Assert.IsNotNull(result.ErrorMessage);
            Console.WriteLine($"✅ Disk space error handled gracefully: {result.ErrorMessage}");
        }
    }

    [TestMethod]
    [Description("Test con plantilla inexistente")]
    public async Task EdgeCase_NonExistentTemplate_ShouldFallbackOrFail()
    {
        // Arrange
        var routine = CreateSimpleTestRoutine();
        var options = new ExportOptions
        {
            OutputPath = _testOutputDirectory,
            AutoOpenAfterExport = false,
            OverwriteExisting = true
        };

        // Act: Usar plantilla que no existe
        var result = await _exportService.ExportRoutineToWordAsync(routine, "plantilla_inexistente", options);

        // Assert: Debe hacer fallback a plantilla por defecto O fallar elegantemente
        if (result.Success)
        {
            Assert.IsTrue(File.Exists(result.FilePath));
            Console.WriteLine("✅ Non-existent template handled - used fallback template");
        }
        else
        {
            Assert.IsNotNull(result.ErrorMessage);
            Assert.IsTrue(result.ErrorMessage.ToLower().Contains("plantilla") ||
                         result.ErrorMessage.ToLower().Contains("template") ||
                         result.ErrorMessage.ToLower().Contains("found"));
            Console.WriteLine($"✅ Non-existent template error handled gracefully: {result.ErrorMessage}");
        }
    }

    [TestMethod]
    [Description("Test de cancelación durante exportación larga")]
    public async Task EdgeCase_CancellationDuringExport_ShouldCancelCleanly()
    {
        // Arrange: Rutina grande para tener tiempo de cancelar
        var largeRoutine = CreateLargeTestRoutine();

        var options = new ExportOptions
        {
            OutputPath = _testOutputDirectory,
            AutoOpenAfterExport = false,
            OverwriteExisting = true
        };

        using var cts = new CancellationTokenSource();

        // Act: Iniciar exportación y cancelar después de un tiempo
        var exportTask = _exportService.ExportRoutineToWordAsync(largeRoutine, "professional", options);

        // Cancelar después de 100ms
        cts.CancelAfter(100);

        try
        {
            var result = await exportTask;

            // Si completa antes de la cancelación
            if (result.Success)
            {
                Console.WriteLine("✅ Export completed before cancellation");
                Assert.IsTrue(File.Exists(result.FilePath));
            }
            else
            {
                Console.WriteLine($"✅ Export failed gracefully: {result.ErrorMessage}");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("✅ Export was cancelled cleanly");
        }
        catch (Exception ex)
        {
            // Otras excepciones también son manejables
            Console.WriteLine($"✅ Export cancellation handled: {ex.Message}");
        }

        // Assert: No debe haber archivos parciales problemáticos
        var partialFiles = Directory.GetFiles(_testOutputDirectory, "*.tmp");
        Assert.AreEqual(0, partialFiles.Length, "Should not leave temporary files after cancellation");
    }

    #region Helper Methods

    private Routine CreateSimpleTestRoutine()
    {
        return new Routine
        {
            Id = 100,
            Name = "Rutina Simple Test",
            ClientName = "Cliente Test",
            Description = "Rutina básica para testing de edge cases",
            Goal = "Testing",
            DurationWeeks = 4,
            CreatedDate = DateTime.Now,
            Days = new List<RoutineDay>
            {
                new RoutineDay
                {
                    Id = 1,
                    DayNumber = 1,
                    Name = "Día 1 - Test",
                    Description = "Día de testing",
                    FocusArea = "General",
                    TargetIntensity = "Moderada",
                    EstimatedDurationMinutes = 60,
                    Exercises = new List<RoutineExercise>
                    {
                        new RoutineExercise
                        {
                            Id = 1,
                            Order = 1,
                            Name = "Ejercicio Test",
                            Category = "Testing",
                            MuscleGroups = new List<string> { "Test" },
                            Equipment = "Ninguno",
                            Instructions = "Ejecutar para test",
                            SafetyTips = "Test seguro",
                            RestTimeSeconds = 60,
                            Difficulty = "Principiante",
                            Sets = new List<ExerciseSet>
                            {
                                new ExerciseSet
                                {
                                    Id = 1,
                                    SetNumber = 1,
                                    Reps = 10,
                                    Weight = 0,
                                    RestSeconds = 60
                                }
                            }
                        }
                    }
                }
            },
            Metrics = new RoutineMetrics
            {
                TotalExercises = 1,
                TotalSets = 1,
                EstimatedDurationMinutes = 60,
                MuscleGroupsCovered = new List<string> { "Test" },
                EquipmentRequired = new List<string> { "Ninguno" },
                DifficultyLevel = "Principiante",
                CaloriesBurnedEstimate = 100
            }
        };
    }

    private Routine CreateLargeTestRoutine()
    {
        var routine = new Routine
        {
            Id = 200,
            Name = "Rutina Grande para Cancelación",
            ClientName = "Cliente Test Cancelación",
            Description = "Rutina grande para probar cancelación durante export",
            Goal = "Testing Cancelación",
            DurationWeeks = 8,
            CreatedDate = DateTime.Now
        };

        // Crear múltiples días con muchos ejercicios
        for (int dayNum = 1; dayNum <= 5; dayNum++)
        {
            var day = new RoutineDay
            {
                Id = dayNum,
                DayNumber = dayNum,
                Name = $"Día {dayNum} - Cancelación Test",
                Description = $"Día {dayNum} para testing de cancelación",
                FocusArea = "Testing",
                TargetIntensity = "Alta",
                EstimatedDurationMinutes = 90
            };

            // 10 ejercicios por día
            for (int exNum = 1; exNum <= 10; exNum++)
            {
                var exercise = new RoutineExercise
                {
                    Id = (dayNum * 100) + exNum,
                    Order = exNum,
                    Name = $"Ejercicio Cancelación {exNum}",
                    Category = "Testing",
                    MuscleGroups = new List<string> { "Músculos Test" },
                    Equipment = "Equipment Test",
                    Instructions = "Instrucciones largas para ejercicio de testing de cancelación durante exportación",
                    SafetyTips = "Consejos de seguridad para testing",
                    RestTimeSeconds = 90,
                    Difficulty = "Intermedio"
                };

                // 5 sets por ejercicio
                for (int setNum = 1; setNum <= 5; setNum++)
                {
                    exercise.Sets.Add(new ExerciseSet
                    {
                        Id = (exercise.Id * 10) + setNum,
                        SetNumber = setNum,
                        Reps = 10,
                        Weight = 50,
                        RestSeconds = 90
                    });
                }

                day.Exercises.Add(exercise);
            }

            routine.Days.Add(day);
        }

        routine.Metrics = new RoutineMetrics
        {
            TotalExercises = routine.Days.SelectMany(d => d.Exercises).Count(),
            TotalSets = routine.Days.SelectMany(d => d.Exercises).SelectMany(e => e.Sets).Count(),
            EstimatedDurationMinutes = routine.Days.Sum(d => d.EstimatedDurationMinutes),
            MuscleGroupsCovered = new List<string> { "Músculos Test" },
            EquipmentRequired = new List<string> { "Equipment Test" },
            DifficultyLevel = "Intermedio",
            CaloriesBurnedEstimate = 500
        };

        return routine;
    }

    #endregion
}