using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GymRoutineGenerator.Core.Models.Routines;
using GymRoutineGenerator.Core.Services.Documents;
using GymRoutineGenerator.Infrastructure.Documents;

namespace GymRoutineGenerator.Tests.Performance;

[TestClass]
public class StressTests
{
    private IExportService _exportService;
    private string _testOutputDirectory;
    private WordDocumentService _wordService;
    private TemplateManagerService _templateService;

    [TestInitialize]
    public void Setup()
    {
        _testOutputDirectory = Path.Combine(Path.GetTempPath(), "GymRoutineStressTests", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
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
    [Description("Stress test: 100 rutinas diferentes exportadas consecutivamente")]
    public async Task Stress_100ConsecutiveExports_ShouldCompleteSuccessfully()
    {
        // Arrange
        var totalExports = 100;
        var successCount = 0;
        var failCount = 0;
        var totalDuration = TimeSpan.Zero;
        var fileSizes = new List<long>();
        var errors = new List<string>();

        Console.WriteLine($"🔥 Starting stress test: {totalExports} consecutive exports");

        var overallStopwatch = Stopwatch.StartNew();

        // Act: Exportar 100 rutinas diferentes
        for (int i = 1; i <= totalExports; i++)
        {
            try
            {
                var routine = CreateVariedRoutine(i);
                var template = GetRandomTemplate(i);

                var options = new ExportOptions
                {
                    OutputPath = _testOutputDirectory,
                    AutoOpenAfterExport = false,
                    OverwriteExisting = true
                };

                var exportStopwatch = Stopwatch.StartNew();
                var result = await _exportService.ExportRoutineToWordAsync(routine, template, options);
                exportStopwatch.Stop();

                if (result.Success)
                {
                    successCount++;
                    totalDuration = totalDuration.Add(exportStopwatch.Elapsed);

                    if (File.Exists(result.FilePath))
                    {
                        var fileInfo = new FileInfo(result.FilePath);
                        fileSizes.Add(fileInfo.Length);
                    }

                    if (i % 10 == 0) // Progress every 10 exports
                    {
                        Console.WriteLine($"   ✅ Progress: {i}/{totalExports} exports completed");
                    }
                }
                else
                {
                    failCount++;
                    errors.Add($"Export {i}: {result.ErrorMessage}");
                    Console.WriteLine($"   ❌ Export {i} failed: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                failCount++;
                errors.Add($"Export {i} exception: {ex.Message}");
                Console.WriteLine($"   💥 Export {i} exception: {ex.Message}");
            }
        }

        overallStopwatch.Stop();

        // Assert
        Assert.IsTrue(successCount >= totalExports * 0.95,
            $"Expected at least 95% success rate, got {successCount}/{totalExports} ({(double)successCount/totalExports:P})");

        // Performance assertions
        var averageExportTime = totalDuration.TotalMilliseconds / successCount;
        Assert.IsTrue(averageExportTime < 5000,
            $"Average export time should be under 5 seconds, got {averageExportTime:F0}ms");

        var overallThroughput = successCount / overallStopwatch.Elapsed.TotalMinutes;
        Assert.IsTrue(overallThroughput > 5,
            $"Should export at least 5 files per minute, got {overallThroughput:F1} files/min");

        // File size consistency
        if (fileSizes.Count > 0)
        {
            var avgSize = fileSizes.Average();
            var minSize = fileSizes.Min();
            var maxSize = fileSizes.Max();

            Assert.IsTrue(minSize > 1000, "All files should be at least 1KB");
            Assert.IsTrue(maxSize < 10 * 1024 * 1024, "No file should exceed 10MB");
        }

        // Report results
        Console.WriteLine($"🎯 STRESS TEST RESULTS:");
        Console.WriteLine($"   ✅ Successful exports: {successCount}/{totalExports} ({(double)successCount/totalExports:P})");
        Console.WriteLine($"   ❌ Failed exports: {failCount}");
        Console.WriteLine($"   ⏱️ Total time: {overallStopwatch.Elapsed:mm\\:ss}");
        Console.WriteLine($"   🚀 Throughput: {overallThroughput:F1} exports/minute");
        Console.WriteLine($"   ⚡ Average export time: {averageExportTime:F0}ms");

        if (fileSizes.Count > 0)
        {
            Console.WriteLine($"   📄 File sizes: {fileSizes.Min()/1024:N0}KB - {fileSizes.Max()/1024:N0}KB (avg: {fileSizes.Average()/1024:N0}KB)");
        }

        if (errors.Count > 0)
        {
            Console.WriteLine($"   🔍 First 5 errors:");
            foreach (var error in errors.Take(5))
            {
                Console.WriteLine($"      - {error}");
            }
        }
    }

    [TestMethod]
    [Description("Memory stress test: crear y exportar rutinas masivas sin memory leaks")]
    public async Task Stress_MemoryUsage_ShouldNotLeak()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        var memoryReadings = new List<long>();
        var exportCount = 20; // Menos exports pero más intensivos

        Console.WriteLine($"🧠 Memory stress test starting - Initial memory: {initialMemory / 1024 / 1024:N0} MB");

        // Act: Crear rutinas progresivamente más grandes
        for (int i = 1; i <= exportCount; i++)
        {
            try
            {
                // Crear rutina cada vez más grande
                var routine = CreateMassiveRoutine(i * 5); // 5, 10, 15, 20... exercises per day

                var options = new ExportOptions
                {
                    OutputPath = _testOutputDirectory,
                    AutoOpenAfterExport = false,
                    OverwriteExisting = true
                };

                var result = await _exportService.ExportRoutineToWordAsync(routine, "professional", options);

                // Force garbage collection and measure memory
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var currentMemory = GC.GetTotalMemory(false);
                memoryReadings.Add(currentMemory);

                if (i % 5 == 0)
                {
                    Console.WriteLine($"   📊 Export {i}: Memory = {currentMemory / 1024 / 1024:N0} MB, " +
                                    $"Growth = +{(currentMemory - initialMemory) / 1024 / 1024:N0} MB");
                }

                Assert.IsTrue(result.Success, $"Export {i} failed: {result.ErrorMessage}");
            }
            catch (OutOfMemoryException)
            {
                Assert.Fail($"Out of memory exception at export {i}");
            }
        }

        // Assert: Memory should not grow excessively
        var finalMemory = memoryReadings.Last();
        var memoryGrowth = finalMemory - initialMemory;
        var maxAcceptableGrowth = 200 * 1024 * 1024; // 200MB max growth

        Assert.IsTrue(memoryGrowth < maxAcceptableGrowth,
            $"Memory growth too high: {memoryGrowth / 1024 / 1024:N0}MB (limit: {maxAcceptableGrowth / 1024 / 1024}MB)");

        // Check for memory leaks pattern (continuously growing memory)
        if (memoryReadings.Count >= 10)
        {
            var firstHalf = memoryReadings.Take(memoryReadings.Count / 2).Average();
            var secondHalf = memoryReadings.Skip(memoryReadings.Count / 2).Average();
            var growthRate = (secondHalf - firstHalf) / firstHalf;

            Assert.IsTrue(growthRate < 0.5,
                $"Potential memory leak detected - memory grew {growthRate:P} between first and second half of test");
        }

        Console.WriteLine($"✅ Memory test passed:");
        Console.WriteLine($"   📈 Total growth: {memoryGrowth / 1024 / 1024:N0} MB");
        Console.WriteLine($"   🎯 Final memory: {finalMemory / 1024 / 1024:N0} MB");
        Console.WriteLine($"   ✨ No memory leaks detected");
    }

    [TestMethod]
    [Description("Concurrent stress test: múltiples exportaciones en paralelo")]
    public async Task Stress_ConcurrentExports_ShouldHandleParallelism()
    {
        // Arrange
        var concurrentExports = 10;
        var routinesPerTask = 5;
        var totalExports = concurrentExports * routinesPerTask;

        Console.WriteLine($"⚡ Concurrent stress test: {concurrentExports} parallel tasks, {routinesPerTask} exports each");

        var overallStopwatch = Stopwatch.StartNew();
        var results = new List<(bool Success, string Error, TimeSpan Duration)>();

        // Act: Lanzar múltiples tareas de exportación en paralelo
        var tasks = Enumerable.Range(1, concurrentExports).Select(async taskId =>
        {
            var taskResults = new List<(bool Success, string Error, TimeSpan Duration)>();

            for (int i = 1; i <= routinesPerTask; i++)
            {
                try
                {
                    var routine = CreateVariedRoutine((taskId * 100) + i);
                    var options = new ExportOptions
                    {
                        OutputPath = Path.Combine(_testOutputDirectory, $"Task{taskId}"),
                        AutoOpenAfterExport = false,
                        OverwriteExisting = true
                    };

                    // Ensure directory exists
                    Directory.CreateDirectory(options.OutputPath);

                    var exportStopwatch = Stopwatch.StartNew();
                    var result = await _exportService.ExportRoutineToWordAsync(routine, "standard", options);
                    exportStopwatch.Stop();

                    taskResults.Add((result.Success, result.ErrorMessage, exportStopwatch.Elapsed));

                    if (!result.Success)
                    {
                        Console.WriteLine($"   ❌ Task {taskId}, Export {i}: {result.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    var duration = TimeSpan.Zero;
                    taskResults.Add((false, ex.Message, duration));
                    Console.WriteLine($"   💥 Task {taskId}, Export {i}: {ex.Message}");
                }
            }

            return taskResults;
        }).ToArray();

        // Wait for all tasks to complete
        var allTaskResults = await Task.WhenAll(tasks);
        overallStopwatch.Stop();

        // Flatten results
        results = allTaskResults.SelectMany(taskResults => taskResults).ToList();

        // Assert
        var successCount = results.Count(r => r.Success);
        var successRate = (double)successCount / results.Count;

        Assert.IsTrue(successRate >= 0.9,
            $"Expected at least 90% success rate in concurrent test, got {successRate:P} ({successCount}/{results.Count})");

        // Performance assertions
        var averageExportTime = results.Where(r => r.Success).Average(r => r.Duration.TotalMilliseconds);
        Assert.IsTrue(averageExportTime < 10000,
            $"Average export time in concurrent test should be under 10 seconds, got {averageExportTime:F0}ms");

        // Concurrency should provide some benefit
        var theoreticalSequentialTime = results.Sum(r => r.Duration.TotalMilliseconds);
        var actualParallelTime = overallStopwatch.Elapsed.TotalMilliseconds;
        var concurrencySpeedup = theoreticalSequentialTime / actualParallelTime;

        Assert.IsTrue(concurrencySpeedup > 2,
            $"Concurrency should provide at least 2x speedup, got {concurrencySpeedup:F1}x");

        Console.WriteLine($"🚀 CONCURRENT TEST RESULTS:");
        Console.WriteLine($"   ✅ Success rate: {successRate:P} ({successCount}/{results.Count})");
        Console.WriteLine($"   ⏱️ Total parallel time: {overallStopwatch.Elapsed:mm\\:ss}");
        Console.WriteLine($"   ⚡ Average export time: {averageExportTime:F0}ms");
        Console.WriteLine($"   🏃 Concurrency speedup: {concurrencySpeedup:F1}x");
        Console.WriteLine($"   📁 Files created in {concurrentExports} subdirectories");
    }

    [TestMethod]
    [Description("Database stress test: rutinas con bases de datos de ejercicios masivas")]
    public async Task Stress_LargeExerciseDatabase_ShouldHandleEfficiently()
    {
        // Arrange: Crear rutinas que simulan una base de datos masiva de ejercicios
        var databaseSizes = new[] { 100, 500, 1000, 2000 }; // Número de ejercicios diferentes
        var results = new List<(int DatabaseSize, bool Success, TimeSpan Duration, long FileSize)>();

        Console.WriteLine("💾 Large exercise database stress test");

        foreach (var dbSize in databaseSizes)
        {
            try
            {
                Console.WriteLine($"   Testing with {dbSize} different exercises...");

                var routine = CreateRoutineWithLargeExerciseDatabase(dbSize);
                var options = new ExportOptions
                {
                    OutputPath = _testOutputDirectory,
                    AutoOpenAfterExport = false,
                    OverwriteExisting = true
                };

                var stopwatch = Stopwatch.StartNew();
                var result = await _exportService.ExportRoutineToWordAsync(routine, "professional", options);
                stopwatch.Stop();

                long fileSize = 0;
                if (result.Success && File.Exists(result.FilePath))
                {
                    fileSize = new FileInfo(result.FilePath).Length;
                }

                results.Add((dbSize, result.Success, stopwatch.Elapsed, fileSize));

                if (result.Success)
                {
                    Console.WriteLine($"      ✅ Success: {stopwatch.Elapsed.TotalSeconds:F1}s, {fileSize/1024:N0}KB");
                }
                else
                {
                    Console.WriteLine($"      ❌ Failed: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                results.Add((dbSize, false, TimeSpan.Zero, 0));
                Console.WriteLine($"      💥 Exception: {ex.Message}");
            }
        }

        // Assert
        var successfulResults = results.Where(r => r.Success).ToList();
        Assert.IsTrue(successfulResults.Count >= databaseSizes.Length * 0.75,
            $"At least 75% of database sizes should succeed, got {successfulResults.Count}/{databaseSizes.Length}");

        // Performance should not degrade exponentially with database size
        if (successfulResults.Count >= 2)
        {
            var firstResult = successfulResults.First();
            var lastResult = successfulResults.Last();

            var sizeRatio = (double)lastResult.DatabaseSize / firstResult.DatabaseSize;
            var timeRatio = lastResult.Duration.TotalMilliseconds / firstResult.Duration.TotalMilliseconds;

            // Time should not grow exponentially with database size
            Assert.IsTrue(timeRatio < sizeRatio * 2,
                $"Performance degradation too high: {timeRatio:F1}x time for {sizeRatio:F1}x database size");
        }

        // File sizes should grow reasonably with content
        var fileSizeGrowth = successfulResults.Max(r => r.FileSize) / (double)successfulResults.Min(r => r.FileSize);
        Assert.IsTrue(fileSizeGrowth < 50,
            $"File size growth seems excessive: {fileSizeGrowth:F1}x between smallest and largest");

        Console.WriteLine($"📊 DATABASE STRESS RESULTS:");
        foreach (var (dbSize, success, duration, fileSize) in results)
        {
            if (success)
            {
                Console.WriteLine($"   📚 {dbSize} exercises: {duration.TotalSeconds:F1}s, {fileSize/1024:N0}KB");
            }
            else
            {
                Console.WriteLine($"   ❌ {dbSize} exercises: FAILED");
            }
        }
    }

    [TestMethod]
    [Description("Long-running stability test: 50 exportaciones durante período extendido")]
    public async Task Stress_LongRunningStability_ShouldMaintainPerformance()
    {
        // Arrange
        var totalRuns = 50;
        var delayBetweenRuns = TimeSpan.FromSeconds(2); // Simular uso real con pauses
        var performanceReadings = new List<(int RunNumber, TimeSpan Duration, long MemoryUsage)>();

        Console.WriteLine($"⏳ Long-running stability test: {totalRuns} exports over extended period");

        var overallStopwatch = Stopwatch.StartNew();

        // Act: Ejecutar exports con delay para simular uso real a largo plazo
        for (int run = 1; run <= totalRuns; run++)
        {
            try
            {
                var routine = CreateVariedRoutine(run);
                var options = new ExportOptions
                {
                    OutputPath = _testOutputDirectory,
                    AutoOpenAfterExport = false,
                    OverwriteExisting = true
                };

                var runStopwatch = Stopwatch.StartNew();
                var result = await _exportService.ExportRoutineToWordAsync(routine, GetRandomTemplate(run), options);
                runStopwatch.Stop();

                // Measure memory after each run
                GC.Collect();
                var memoryUsage = GC.GetTotalMemory(false);

                performanceReadings.Add((run, runStopwatch.Elapsed, memoryUsage));

                Assert.IsTrue(result.Success, $"Run {run} failed: {result.ErrorMessage}");

                if (run % 10 == 0)
                {
                    Console.WriteLine($"   🏃 Run {run}/{totalRuns} completed - " +
                                    $"Time: {runStopwatch.Elapsed.TotalSeconds:F1}s, " +
                                    $"Memory: {memoryUsage / 1024 / 1024:N0}MB");
                }

                // Wait between runs to simulate real usage pattern
                if (run < totalRuns)
                {
                    await Task.Delay(delayBetweenRuns);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Long-running test failed at run {run}: {ex.Message}");
            }
        }

        overallStopwatch.Stop();

        // Assert: Performance should remain stable over time
        var firstQuarter = performanceReadings.Take(performanceReadings.Count / 4);
        var lastQuarter = performanceReadings.TakeLast(performanceReadings.Count / 4);

        var avgTimeFirst = firstQuarter.Average(r => r.Duration.TotalMilliseconds);
        var avgTimeLast = lastQuarter.Average(r => r.Duration.TotalMilliseconds);
        var timePerformanceDrift = Math.Abs(avgTimeLast - avgTimeFirst) / avgTimeFirst;

        Assert.IsTrue(timePerformanceDrift < 0.3,
            $"Performance drift too high: {timePerformanceDrift:P} between first and last quarter");

        var avgMemoryFirst = firstQuarter.Average(r => r.MemoryUsage);
        var avgMemoryLast = lastQuarter.Average(r => r.MemoryUsage);
        var memoryDrift = (avgMemoryLast - avgMemoryFirst) / avgMemoryFirst;

        Assert.IsTrue(memoryDrift < 0.5,
            $"Memory drift too high: {memoryDrift:P} growth from first to last quarter");

        // No individual run should be an outlier (more than 3x average)
        var avgDuration = performanceReadings.Average(r => r.Duration.TotalMilliseconds);
        var outliers = performanceReadings.Where(r => r.Duration.TotalMilliseconds > avgDuration * 3).ToList();

        Assert.IsTrue(outliers.Count <= totalRuns * 0.05,
            $"Too many outlier runs: {outliers.Count} (max allowed: {totalRuns * 0.05:F0})");

        Console.WriteLine($"⏱️ STABILITY TEST RESULTS:");
        Console.WriteLine($"   🎯 Total runs: {totalRuns} (100% success)");
        Console.WriteLine($"   ⏳ Total duration: {overallStopwatch.Elapsed:hh\\:mm\\:ss}");
        Console.WriteLine($"   📈 Performance drift: {timePerformanceDrift:P}");
        Console.WriteLine($"   🧠 Memory drift: {memoryDrift:P}");
        Console.WriteLine($"   🎪 Outliers: {outliers.Count}/{totalRuns}");
        Console.WriteLine($"   ⚡ Average export time: {avgDuration:F0}ms");
    }

    #region Helper Methods

    private Routine CreateVariedRoutine(int seed)
    {
        var random = new Random(seed);
        var goals = new[] { "Fuerza", "Resistencia", "Flexibilidad", "General", "Rehabilitación" };
        var intensities = new[] { "Baja", "Moderada", "Alta", "Muy Alta" };

        var routine = new Routine
        {
            Id = seed,
            Name = $"Rutina Variada #{seed}",
            ClientName = $"Cliente Test {seed}",
            Description = $"Rutina generada automáticamente para stress testing - Seed {seed}",
            Goal = goals[seed % goals.Length],
            DurationWeeks = 4 + (seed % 8),
            CreatedDate = DateTime.Now.AddDays(-random.Next(365))
        };

        // Variable number of days (2-6)
        var dayCount = 2 + (seed % 5);
        for (int dayNum = 1; dayNum <= dayCount; dayNum++)
        {
            var day = new RoutineDay
            {
                Id = dayNum,
                DayNumber = dayNum,
                Name = $"Día {dayNum} - Variado {seed}",
                Description = $"Día de entrenamiento generado para testing - Rutina {seed}",
                FocusArea = GetRandomFocusArea(seed + dayNum),
                TargetIntensity = intensities[(seed + dayNum) % intensities.Length],
                EstimatedDurationMinutes = 45 + random.Next(60)
            };

            // Variable number of exercises (3-12)
            var exerciseCount = 3 + (seed % 10);
            for (int exNum = 1; exNum <= exerciseCount; exNum++)
            {
                var exercise = CreateVariedExercise(seed, dayNum, exNum);
                day.Exercises.Add(exercise);
            }

            routine.Days.Add(day);
        }

        routine.Metrics = CalculateMetrics(routine);
        return routine;
    }

    private Routine CreateMassiveRoutine(int exercisesPerDay)
    {
        var routine = new Routine
        {
            Id = exercisesPerDay * 1000,
            Name = $"Rutina Masiva - {exercisesPerDay} ejercicios/día",
            ClientName = "Cliente Stress Test Masivo",
            Description = new string('M', 2000), // 2KB description
            Goal = "Testing de Memoria",
            DurationWeeks = 12,
            CreatedDate = DateTime.Now
        };

        // 5 días de entrenamiento
        for (int dayNum = 1; dayNum <= 5; dayNum++)
        {
            var day = new RoutineDay
            {
                Id = dayNum,
                DayNumber = dayNum,
                Name = $"Día Masivo {dayNum}",
                Description = new string('D', 1000), // 1KB description per day
                FocusArea = $"Área Masiva {dayNum}",
                TargetIntensity = "Extrema",
                EstimatedDurationMinutes = 120
            };

            // Many exercises per day
            for (int exNum = 1; exNum <= exercisesPerDay; exNum++)
            {
                var exercise = new RoutineExercise
                {
                    Id = (dayNum * 1000) + exNum,
                    Order = exNum,
                    Name = $"Ejercicio Masivo {dayNum}.{exNum}",
                    Category = "Stress Testing",
                    MuscleGroups = Enumerable.Range(1, 5).Select(i => $"Músculo {i}").ToList(),
                    Equipment = new string('E', 200), // 200 chars equipment
                    Instructions = new string('I', 1000), // 1KB instructions
                    SafetyTips = new string('S', 500), // 500 chars safety
                    RestTimeSeconds = 120,
                    Difficulty = "Extremo"
                };

                // 6 sets per exercise
                for (int setNum = 1; setNum <= 6; setNum++)
                {
                    exercise.Sets.Add(new ExerciseSet
                    {
                        Id = (exercise.Id * 10) + setNum,
                        SetNumber = setNum,
                        Reps = 8 + setNum,
                        Weight = 50 + (setNum * 5),
                        RestSeconds = 120
                    });
                }

                day.Exercises.Add(exercise);
            }

            routine.Days.Add(day);
        }

        routine.Metrics = CalculateMetrics(routine);
        return routine;
    }

    private Routine CreateRoutineWithLargeExerciseDatabase(int totalExercises)
    {
        var routine = new Routine
        {
            Id = totalExercises,
            Name = $"Rutina con {totalExercises} Ejercicios Únicos",
            ClientName = "Cliente Database Test",
            Description = $"Rutina para probar base de datos con {totalExercises} ejercicios diferentes",
            Goal = "Testing Database",
            DurationWeeks = 8,
            CreatedDate = DateTime.Now
        };

        // Distribute exercises across multiple days
        var daysNeeded = Math.Min(7, Math.Max(1, totalExercises / 20)); // Max 7 days, min 1
        var exercisesPerDay = totalExercises / daysNeeded;
        var remainingExercises = totalExercises % daysNeeded;

        for (int dayNum = 1; dayNum <= daysNeeded; dayNum++)
        {
            var day = new RoutineDay
            {
                Id = dayNum,
                DayNumber = dayNum,
                Name = $"Día DB {dayNum}",
                Description = $"Día con múltiples ejercicios únicos de la base de datos",
                FocusArea = "Database Testing",
                TargetIntensity = "Moderada",
                EstimatedDurationMinutes = Math.Min(180, exercisesPerDay * 3) // Max 3 hours
            };

            var exercisesThisDay = exercisesPerDay;
            if (dayNum <= remainingExercises)
                exercisesThisDay++; // Distribute remainder

            for (int exNum = 1; exNum <= exercisesThisDay; exNum++)
            {
                var globalExerciseId = ((dayNum - 1) * exercisesPerDay) + exNum + Math.Min(dayNum - 1, remainingExercises);
                var exercise = CreateUniqueExercise(globalExerciseId);
                exercise.Order = exNum;
                day.Exercises.Add(exercise);
            }

            routine.Days.Add(day);
        }

        routine.Metrics = CalculateMetrics(routine);
        return routine;
    }

    private RoutineExercise CreateVariedExercise(int seed, int dayNum, int exNum)
    {
        var random = new Random(seed + dayNum + exNum);
        var exerciseNames = new[]
        {
            "Press de Banca", "Sentadillas", "Peso Muerto", "Dominadas", "Press Militar",
            "Remo con Barra", "Curl de Bíceps", "Extensiones Tríceps", "Elevaciones Laterales",
            "Plancha", "Abdominales", "Fondos", "Lunges", "Hip Thrust"
        };

        var exercise = new RoutineExercise
        {
            Id = (dayNum * 1000) + exNum,
            Order = exNum,
            Name = exerciseNames[random.Next(exerciseNames.Length)] + $" Var{seed}",
            Category = GetRandomCategory(seed + exNum),
            MuscleGroups = GetRandomMuscleGroups(seed + dayNum + exNum),
            Equipment = GetRandomEquipment(seed + exNum),
            Instructions = $"Instrucciones detalladas para {exerciseNames[random.Next(exerciseNames.Length)]} - Generado automáticamente para testing",
            SafetyTips = "Consejos de seguridad generados automáticamente para stress testing",
            RestTimeSeconds = 60 + random.Next(120),
            Difficulty = GetRandomDifficulty(seed + exNum)
        };

        // Variable number of sets (2-6)
        var setCount = 2 + random.Next(5);
        for (int setNum = 1; setNum <= setCount; setNum++)
        {
            exercise.Sets.Add(new ExerciseSet
            {
                Id = (exercise.Id * 10) + setNum,
                SetNumber = setNum,
                Reps = 8 + random.Next(12),
                Weight = 20 + random.Next(80),
                RestSeconds = exercise.RestTimeSeconds
            });
        }

        return exercise;
    }

    private RoutineExercise CreateUniqueExercise(int exerciseId)
    {
        // Create truly unique exercise for database testing
        var categories = new[] { "Fuerza", "Cardio", "Flexibilidad", "Funcional", "Rehabilitación" };
        var difficulties = new[] { "Principiante", "Intermedio", "Avanzado", "Experto" };

        var exercise = new RoutineExercise
        {
            Id = exerciseId,
            Order = 1,
            Name = $"Ejercicio Único DB #{exerciseId:D4}",
            Category = categories[exerciseId % categories.Length],
            MuscleGroups = new List<string> { $"Músculo Específico {exerciseId}" },
            Equipment = $"Equipment Específico {exerciseId}",
            Instructions = $"Instrucciones específicas para ejercicio único #{exerciseId} - " + new string('I', 100),
            SafetyTips = $"Consejos específicos para ejercicio #{exerciseId}",
            RestTimeSeconds = 60 + (exerciseId % 120),
            Difficulty = difficulties[exerciseId % difficulties.Length]
        };

        // 3-4 sets per exercise
        var setCount = 3 + (exerciseId % 2);
        for (int setNum = 1; setNum <= setCount; setNum++)
        {
            exercise.Sets.Add(new ExerciseSet
            {
                Id = (exerciseId * 10) + setNum,
                SetNumber = setNum,
                Reps = 8 + (exerciseId % 8),
                Weight = 40 + (exerciseId % 60),
                RestSeconds = exercise.RestTimeSeconds
            });
        }

        return exercise;
    }

    private string GetRandomTemplate(int seed)
    {
        var templates = new[] { "basic", "standard", "professional", "gym", "rehabilitation" };
        return templates[seed % templates.Length];
    }

    private string GetRandomFocusArea(int seed)
    {
        var areas = new[] { "Pecho", "Espalda", "Piernas", "Hombros", "Brazos", "Core", "Cuerpo Completo" };
        return areas[seed % areas.Length];
    }

    private string GetRandomCategory(int seed)
    {
        var categories = new[] { "Fuerza", "Cardio", "Flexibilidad", "Funcional" };
        return categories[seed % categories.Length];
    }

    private List<string> GetRandomMuscleGroups(int seed)
    {
        var allMuscles = new[] { "Pectorales", "Dorsales", "Deltoides", "Bíceps", "Tríceps", "Cuádriceps", "Isquiotibiales", "Glúteos", "Core" };
        var random = new Random(seed);
        var count = 1 + random.Next(3); // 1-3 muscle groups
        return allMuscles.OrderBy(x => random.Next()).Take(count).ToList();
    }

    private string GetRandomEquipment(int seed)
    {
        var equipment = new[] { "Barra", "Mancuernas", "Máquina", "Peso Corporal", "Bandas", "Kettlebell" };
        return equipment[seed % equipment.Length];
    }

    private string GetRandomDifficulty(int seed)
    {
        var difficulties = new[] { "Principiante", "Intermedio", "Avanzado" };
        return difficulties[seed % difficulties.Length];
    }

    private RoutineMetrics CalculateMetrics(Routine routine)
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
            DifficultyLevel = "Variado",
            CaloriesBurnedEstimate = routine.Days.Sum(d => d.EstimatedDurationMinutes) * 6
        };
    }

    #endregion
}